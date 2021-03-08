using DG.Tweening;
using UnityEngine;

public class PlayerGripManager : MonoBehaviour {
    public enum GripMotionState { Ready, InMotion, LiftingUp }

    [SerializeField] LayerMask _gripLayer;
    [SerializeField] LayerMask _attackLayer;

    [Header("Grip sockets")]
    [SerializeField] Transform _gripSocket;
    [SerializeField] Transform _gripPositionDefault;
    [SerializeField] Transform _gripPositionDown;
    [SerializeField] Transform _gripPositionThrow;
    [SerializeField] Transform _gripPositionPunchLoad;
    [SerializeField] Transform _gripPositionPunchFinish;
    [SerializeField] Transform _gripPositionUseHold;
    [SerializeField] Transform _gripPositionUseTwist;

    [Header("Grip stats")]
    [Tooltip("How far in front to try and place an item you gently set down")]
    [SerializeField] float _maxRaycastDropDistance = 1.5f;
    [SerializeField] Transform _gripThrowOrigin;
    [SerializeField] float _throwForce = 10f;
    [SerializeField, Range(0, 1f)] float _throwTime = .15f;
    [SerializeField] float _gripRange = 3f;
    [SerializeField] float _gripRadius = .5f;
    [SerializeField, Range(0, 1f)] float _punchTime = .25f;
    [SerializeField] float _cooldownTime = .2f;
    [SerializeField] float _useTime = .4f;
    [SerializeField] AnimationCurve _attackCurve = new AnimationCurve();

    [Header("Movement Bob")]
    [Tooltip("Frequency at which the fist will move around in the screen when the player is in movement")]
    [SerializeField] float _bobFrequency = 10f;
    [Tooltip("How fast movement bob is applied, the bigger value the fastest")]
    [SerializeField] float _bobSharpness = 10f;
    [Tooltip("Distance the fist bobs when not aiming")]
    [SerializeField] float _defaultBobAmount = 0.05f;
    [Tooltip("Distance the fist bobs when aiming")]
    [SerializeField] float _raisingBobAmount = 0.02f;

    [Header("Fist Recoil")]
    [Tooltip("This will affect how fast the recoil moves the fist, the bigger the value, the fastest")]
    public float recoilSharpness = 50f;
    [Tooltip("Maximum distance the recoil can affect the fist")]
    public float maxRecoilDistance = 0.5f;
    [Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
    public float recoilRestitutionSharpness = 10f;

    [Header("Punching")]
    [SerializeField] GameObject _punchFx;
    [SerializeField] AudioClip _punchSfx;
    [SerializeField] float _punchDamage = 50f;
    [SerializeField] float _punchKnockback = 10f;

    // grip privates
    Grippable _currentlyGrippedThing;
    Vector3 _mainSocketLocalPosition = Vector3.zero; // main target for tweened animation
    Vector3 _mainSocketBobLocalPosition = Vector3.zero;
    Vector3 _mainSocketRecoilLocalPosition = Vector3.zero;
    Vector3 m_AccumulatedRecoil = Vector3.zero;
    GripMotionState _gripMotionState = GripMotionState.Ready;
    float _movementBobFactor;

    // dependencies 
    PlayerInputHandler m_InputHandler;
    PlayerWeaponsManager _playerWeaponsManager;
    PlayerCharacterController _playerController;
    Damageable _damage;
    AudioSource _audio;

    // etc
    public bool IsEmptyHanded { get { return _currentlyGrippedThing == null; } }

    void Start() {
        m_InputHandler = GetComponent<PlayerInputHandler>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerGripManager>(m_InputHandler, this, gameObject);

        _playerWeaponsManager = GetComponent<PlayerWeaponsManager>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerWeaponsManager, PlayerGripManager>(_playerWeaponsManager, this, gameObject);

        _playerController = GetComponent<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerGripManager>(_playerController, this, gameObject);

        _damage = GetComponent<Damageable>();
        DebugUtility.HandleErrorIfNullGetComponent<Damageable, PlayerGripManager>(_damage, this, gameObject);

        _audio = GetComponent<AudioSource>();
        DebugUtility.HandleErrorIfNullGetComponent<AudioSource, PlayerGripManager>(_audio, this, gameObject);

        _mainSocketLocalPosition = _gripPositionDefault.localPosition;
    }

    void Update() {
        HandleInputs();
    }

    void HandleInputs() {

        // empty handed
        if (IsEmptyHanded) {
            if (!FistMotionIsReady()) { return; }
            // swing
            if (m_InputHandler.GetPunchInputDown()) { BeginPunch(); }

            if (m_InputHandler.GetGripInputDown()) { TryForwardGrab(); }
        }

        // gripped something
        else {
            var useInputDown = m_InputHandler.GetGripInputDown();
            var useInputHeld = m_InputHandler.GetGripInputHeld();
            var gripInputDown = m_InputHandler.GetPunchInputDown();
            var throwInputDown = m_InputHandler.GetThrowInputDown();
            var punchInputDown = m_InputHandler.GetPunchInputDown();

            // throw thing
            if (throwInputDown) {
                if (!FistMotionIsReady()) { return; }

                // set down if crouching
                if (_playerController.isCrouching) { SetDownSequence(); }

                // throw it
                else { ThrowGrippedThing(); }
            }

            // swing thing
            else if (punchInputDown) {
                if (!FistMotionIsReady()) { return; }
                var gripMelee = _currentlyGrippedThing.GetComponent<GripMelee>();
                if (gripMelee != null) {
                    BeginCustomGrippedMeleeSwing(gripMelee);
                } else {
                    BeginPunch();
                }
            }

            // activate thing
            else { UpdateUseInput(useInputDown, useInputHeld); }
        }
    }

    // Update various animated features in LateUpdate because it needs to override the animated arm position
    void LateUpdate() {
        UpdateGripRecoil();
        UpdateMovementBob();

        // Set final weapon socket position based on all the combined animation influences
        _gripSocket.localPosition = _mainSocketLocalPosition + _mainSocketBobLocalPosition + _mainSocketRecoilLocalPosition;

        // grippables with rigidbodies will drift away unless their position is manually updated
        TrackGrippedPosition();
    }

    void TrackGrippedPosition() {
        if (!IsEmptyHanded) {
            _currentlyGrippedThing.transform.position = _gripSocket.position;
            _currentlyGrippedThing.transform.rotation = _gripSocket.rotation;

            // get the offset vector from the body socket to the item's socket
            var socketOffset = _gripSocket.position - _currentlyGrippedThing.gripPoint.position;

            // shift item by that amount to line up
            _currentlyGrippedThing.transform.position += socketOffset;
        }
    }

    void TryForwardGrab() {
        // prioritize directly targeted
        var precise = PreciseRaycast(_gripLayer);
        if (precise != null) {
            if (TryGrip(precise)) return;
            if (TryPressButton(precise)) return;
            if (TryUse(precise)) return;
        }

        // expanded targets are tested at the same priority
        var expanded = ExpandedRaycast(_gripLayer);
        if (expanded != null) {
            if (TryGrip(expanded)) return;
            if (TryPressButton(expanded)) return;
            if (TryUse(expanded)) return;
        }
    }

    void RecoilFist(float recoilForce) {
        m_AccumulatedRecoil += Vector3.back * recoilForce * .65f;
        m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, maxRecoilDistance);
    }

    void UpdateGripRecoil() {
        // if the accumulated recoil is further away from the current position, make the current position move towards the recoil target
        if (_mainSocketRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f) {
            _mainSocketRecoilLocalPosition = Vector3.Lerp(_mainSocketRecoilLocalPosition, m_AccumulatedRecoil, recoilSharpness * Time.deltaTime);
        }
        // otherwise, move recoil position to make it recover towards its resting pose
        else {
            _mainSocketRecoilLocalPosition = Vector3.Lerp(_mainSocketRecoilLocalPosition, Vector3.zero, recoilRestitutionSharpness * Time.deltaTime);
            m_AccumulatedRecoil = _mainSocketRecoilLocalPosition;
        }
    }

    void UpdateMovementBob() {
        _movementBobFactor = Mathf.Lerp(_movementBobFactor, _playerController.characterMovementFactor, _bobSharpness * Time.deltaTime);

        // Calculate vertical and horizontal weapon bob values based on a sine function
        float bobAmount = !FistMotionIsReady() ? _raisingBobAmount : _defaultBobAmount;
        float frequency = _bobFrequency;
        float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * _movementBobFactor;
        float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount * _movementBobFactor;

        // Apply weapon bob
        _mainSocketBobLocalPosition.x = hBobValue;
        _mainSocketBobLocalPosition.y = Mathf.Abs(vBobValue);
    }

    void UpdateUseInput(bool useButtonDown, bool useButtonHeld) {

        switch (_currentlyGrippedThing.activationType) {
            case Grippable.ActivationType.Hold:
                UpdateHoldTypeInput(useButtonDown, useButtonHeld);
                break;

            case Grippable.ActivationType.Press:
                UpdateUseConsumableInput(useButtonDown);
                break;

            default:
                UpdateUselessInput(useButtonDown);
                break;
        }
    }

    void UpdateHoldTypeInput(bool useButtonDown, bool useButtonHeld) {
        if (FistMotionIsReady() && useButtonDown) {
            BeginUseHoldSequence();
            // items are lifted while using, releasing means stop using
        } else if (FistMotionIsLifting() && !useButtonHeld) {
            EndUseHoldSequence();
        }
    }

    void UpdateUseConsumableInput(bool useButtonDown) {
        if (FistMotionIsReady() && useButtonDown) {
            UseConsumableSequence();
        }
    }

    void UpdateUselessInput(bool useButtonDown) {
        if (FistMotionIsReady() && useButtonDown) {
            DoNothingWobbleSequence();
        }
    }

    void DoNothingWobbleSequence() {
        var leftTwist = _gripPositionDefault.localEulerAngles;
        var rightTwist = _gripPositionDefault.localEulerAngles;

        leftTwist.z -= 45f;
        leftTwist.x -= 30f;
        rightTwist.z += 45f;
        rightTwist.z += 30f;

        DOTween.Sequence()
            .AppendCallback(SetFistMoving)
            .Append(RotateFistSequence(leftTwist, .15f))
            .Append(RotateFistSequence(rightTwist, .15f))
            .Append(ResetFistPosition(.15f))
            .OnComplete(SetFistReady);
    }

    void BeginCustomGrippedMeleeSwing(GripMelee meleeGrippable) {
        meleeGrippable.MeleeSequence(TransformFistSequence, ResetFistPosition, BeginPunchSequence, ForwardPunch, EndPunchSequence);
    }

    void BeginPunch() {
        DOTween.Sequence()
            .AppendCallback(BeginPunchSequence)
            .Append(TransformFistSequence(_gripPositionPunchLoad, _punchTime * .25f, _attackCurve)) // punching
            .AppendCallback(ForwardPunch)
            .Append(TransformFistSequence(_gripPositionPunchFinish, _punchTime * .75f, _attackCurve)) // punching
            .Append(ResetFistPosition()) // recovering
            .OnComplete(EndPunchSequence);
    }

    void BeginPunchSequence() {
        SetFistMoving();
        _playerWeaponsManager.LowerWeapon();
    }

    void EndPunchSequence() {
        SetFistReady();
        _playerWeaponsManager.RaiseWeapon();
    }

    void SetFistReady() {
        _gripMotionState = GripMotionState.Ready;
    }

    void SetFistMoving() {
        _gripMotionState = GripMotionState.InMotion;
    }

    void ForwardPunch() {
        // prioritize directly targeted
        var precise = PreciseRaycast(_attackLayer);
        if (precise != null) {
            if (TryDamaging(precise, _punchDamage)) {
                PunchFx(precise.transform.position);
                return;
            }
        }

        // TODO -- clean up layers, some components on different layers as others with same purpose
        precise = PreciseRaycast(_gripLayer);
        if (precise != null) {
            if (TryPressButton(precise)) {
                PunchFx(precise.transform.position);
                return;
            }
            if (TryDamaging(precise, _punchDamage)) {
                PunchFx(precise.transform.position);
                return;
            }
        }

        // expanded targets are tested at the same priority
        var expanded = ExpandedRaycast(_attackLayer);
        if (expanded != null) {
            if (TryDamaging(expanded, _punchDamage)) {
                PunchFx(expanded.transform.position);
                return;
            }
        }

        expanded = ExpandedRaycast(_gripLayer);
        if (expanded != null) {
            if (TryPressButton(expanded)) {
                PunchFx(expanded.transform.position);
                return;
            }
            if (TryDamaging(expanded, _punchDamage)) {
                PunchFx(expanded.transform.position);
                return;
            }
        }
    }

    void PunchFx(Vector3 punchPos) {
        if (_audio && _punchSfx) {
            _audio.PlayOneShot(_punchSfx);
        }

        if (_punchFx) {
            Instantiate(_punchFx, punchPos, Quaternion.identity);
        }
    }

    void Grip(Grippable gripped) {
        ClaimGrippable(gripped);

        // play a short pickup motion
        MoveFistFromSequence(_gripPositionDown, _gripPositionDefault, .25f);
    }

    void ClaimGrippable(Grippable gripped) {
        _currentlyGrippedThing = gripped;

        // parent the socket and change its old reference
        gripped.transform.SetParent(_gripSocket.transform, true);

        // gripped thing reacts to this
        gripped.BecomeGripped(_playerController.gameObject, _playerWeaponsManager.GetWeaponLayerIndex());

        // subscribe to recoil
        _currentlyGrippedThing.onRecoilReceived.AddListener(RecoilFist);
    }

    // BUG: throwing forward often stops forward motion due to hitting the collider
    void ThrowGrippedThing() {
        DOTween.Sequence()
            .AppendCallback(SetFistMoving)
            .Append(TransformFistSequence(_gripPositionThrow, _throwTime, _attackCurve)) // throw
            .AppendCallback(DoThrow)
            .Append(ResetFistPosition()) // recover
            .OnComplete(() => SetFistReady());
    }

    void DoThrow() {
        var gripped = RaycastAndDropGrippable();
        if (gripped != null) {
            gripped.Throw(_playerWeaponsManager.shotDirection, _throwForce);
        }
    }

    // Raycasts forward and drops the grippable
    Grippable RaycastAndDropGrippable() {
        var paddingDist = .25f;
        var rayDirection = _playerWeaponsManager.shotDirection;
        var rayOrigin = _playerController.playerCamera.transform.position - rayDirection * paddingDist;
        var checkDist = _maxRaycastDropDistance + paddingDist;

        var dropped = UnGrip();
        var droppedRb = dropped.GetComponent<Rigidbody>();
        Vector3 finalPosition = rayOrigin;

        // TODO -- pressing against a wall sometimes lets player throw things through it
        // move the rb to the camera position and sweep test in aim direction for obstruction
        droppedRb.transform.position = rayOrigin;

        if (droppedRb.SweepTest(rayDirection, out RaycastHit hitInfo, checkDist)) {
            // object pushed back by object
            finalPosition += rayDirection * hitInfo.distance;
            //Debug.Log("obstructed!");
        } else {
            // object at exact drop distance
            finalPosition += rayDirection * checkDist;
        }

        droppedRb.transform.position = finalPosition;
        return dropped;
    }

    Grippable UnGrip() {
        _currentlyGrippedThing.onRecoilReceived.RemoveListener(RecoilFist);
        var ungrippedThing = _currentlyGrippedThing;

        // clear parent transforms and clear data
        _currentlyGrippedThing.transform.SetParent(null);
        _currentlyGrippedThing.UnGrip();
        _currentlyGrippedThing = null;

        return ungrippedThing;
    }

    // helpers
    GameObject PreciseRaycast(LayerMask layerMask) {
        var lookOrigin = _playerWeaponsManager.lookPosition;
        var lookDirection = _playerWeaponsManager.shotDirection;

        if (Physics.Raycast(lookOrigin, lookDirection,
                out RaycastHit rayHit, _gripRange, layerMask)) {
            return rayHit.collider.gameObject;
        }
        return null;
    }

    GameObject ExpandedRaycast(LayerMask layerMask) {
        var lookOrigin = _playerWeaponsManager.lookPosition;
        var lookDirection = _playerWeaponsManager.shotDirection;

        // spherecast shouldn't reach behind player, offset it forward by its radius
        var sphereCenter = lookOrigin + lookDirection * _gripRadius;

        // sphere check shouldn't extend further than the regular ray check
        var sphereDistance = Mathf.Max(_gripRange - _gripRadius, 0);

        // if nothing is raycast, check a small distance on all sides for a nearby item
        if (Physics.SphereCast(sphereCenter, _gripRadius, lookDirection,
                out RaycastHit sphereHit, sphereDistance, layerMask)) {
            return sphereHit.collider.gameObject;
        }

        return null;
    }

    bool GrippingHoldType() {
        return _currentlyGrippedThing.activationType == Grippable.ActivationType.Hold;
    }

    bool GrippingPressType() {
        return _currentlyGrippedThing.activationType == Grippable.ActivationType.Press;
    }

    bool FistMotionIsLifting() {
        return _gripMotionState == GripMotionState.LiftingUp;
    }

    bool FistMotionIsReady() {
        return _gripMotionState == GripMotionState.Ready;
    }

    bool TryGrip(GameObject potential) {
        var grippable = potential.GetComponent<Grippable>();
        if (grippable != null) {
            Grip(grippable);
            return true;
        }
        return false;
    }

    bool TryPressButton(GameObject potential) {
        var button = potential.GetComponent<PunchButton>();
        if (button != null) {
            button.PressButton();
            return true;
        }
        return false;
    }

    bool TryUse(GameObject potential) {
        var usable = potential.GetComponent<Usable>();
        if (usable != null) {
            usable.Use();
            return true;
        }
        return false;
    }

    bool TryDamaging(GameObject potential, float damage) {
        var target = potential.GetComponent<Damageable>();
        if (target != null) {
            target.InflictDamage(damage, false, _playerController.gameObject, transform.position);
            return true;
        }
        return false;
    }

    // Tweening stuff
    void SetDownSequence() {
        MoveFistFromSequence(_gripPositionDown, _gripPositionDefault, .1f)
            .OnComplete(() => RaycastAndDropGrippable());
    }

    void BeginUseHoldSequence() {
        DOTween.Sequence()
            .AppendCallback(_currentlyGrippedThing.ActivateBegin) // feels responsive to activate immediately
            .AppendCallback(SetFistMoving)
            .Append(TransformFistSequence(_gripPositionUseHold, _punchTime))
            .OnComplete(() => _gripMotionState = GripMotionState.LiftingUp);
    }

    void EndUseHoldSequence() {
        DOTween.Sequence()
            .AppendCallback(SetFistMoving)
            .Append(ResetFistPosition())
            .AppendCallback(_currentlyGrippedThing.ActivateEnd)
            .OnComplete(SetFistReady);
    }

    void UseConsumableSequence() {
        DOTween.Sequence()
            .AppendCallback(SetFistMoving)
            .Append(TransformFistSequence(_gripPositionUseHold, _useTime / 2))
            .AppendCallback(_currentlyGrippedThing.ActivateBegin)
            .Append(TransformFistSequence(_gripPositionUseTwist, _useTime / 2))
            .Append(ResetFistPosition())
            .OnComplete(SetFistReady);
    }

    Tween ResetFistPosition(float duration) {
        return TransformFistSequence(_gripPositionDefault, duration);
    }

    Tween ResetFistPosition() {
        return ResetFistPosition(_cooldownTime);
    }

    Tween TransformFistSequence(Transform goal, float duration) {
        Sequence s = DOTween.Sequence()
            // position over time
            .Append(DOTween.To(() => _mainSocketLocalPosition,
                localPos => _mainSocketLocalPosition = localPos,
                goal.localPosition,
                duration))
            // simultaneous rotation over same time
            .Join(_gripSocket.DOLocalRotateQuaternion(goal.localRotation, duration));

        return s;
    }

    Tween RotateFistSequence(Vector3 targetEulers, float duration) {
        return _gripSocket.DOLocalRotate(targetEulers, duration);
    }

    Tween TransformFistSequence(Transform target, float duration, AnimationCurve curve) {
        return TransformFistSequence(target, duration).SetEase(curve);
    }

    Tween MoveFistFromSequence(Transform snapTo, Transform moveTo, float duration) {
        // teleport
        _mainSocketLocalPosition = snapTo.localPosition;
        _gripSocket.localEulerAngles = snapTo.localEulerAngles;

        // move
        return TransformFistSequence(moveTo, duration);
    }

    Tween MoveFistFromSequence(Transform snapTo, Transform moveTo, float duration, AnimationCurve curve) {
        return MoveFistFromSequence(snapTo, moveTo, duration).SetEase(curve);
    }
}