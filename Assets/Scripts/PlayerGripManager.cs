using DG.Tweening;
using UnityEngine;

public class PlayerGripManager : MonoBehaviour {
    public enum GripMotionState { Ready, InMotion, LiftingUp }

    [SerializeField] LayerMask _gripLayer;

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
    [SerializeField] Transform _gripThrowOrigin;
    [SerializeField] float _throwForce = 10f;
    [SerializeField, Range(0, 1f)] float _throwTime = .15f;
    [SerializeField] float _gripRange = 3f;
    [SerializeField] float _gripRadius = .5f;
    [SerializeField, Range(0, 1f)] float _punchTime = .25f;
    [SerializeField] float _cooldownTime = .2f;
    [SerializeField] float _useTime = .4f;
    [SerializeField] AnimationCurve _attackCurve = new AnimationCurve();

    [Header("Fist Recoil")]
    [Tooltip("This will affect how fast the recoil moves the fist, the bigger the value, the fastest")]
    public float recoilSharpness = 50f;
    [Tooltip("Maximum distance the recoil can affect the fist")]
    public float maxRecoilDistance = 0.5f;
    [Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
    public float recoilRestitutionSharpness = 10f;

    // grip privates
    Grippable _currentlyGrippedThing;
    Vector3 _mainSocketLocalPosition = Vector3.zero; // main target for tweened animation
    Vector3 _mainSocketBobLocalPosition = Vector3.zero;
    Vector3 _mainSocketRecoilLocalPosition = Vector3.zero;
    Vector3 m_AccumulatedRecoil = Vector3.zero;
    //bool _fistInMotion = false; // can't perform other actions if busy
    GripMotionState _gripMotionState = GripMotionState.Ready;

    // dependencies 
    PlayerInputHandler m_InputHandler;
    PlayerWeaponsManager _playerWeaponsManager;
    PlayerCharacterController _playerController;
    Damageable _damage;

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
        DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, Damageable>(_damage, this, gameObject);

        _damage.onDamageBlocked.AddListener(RecoilFist);
        _mainSocketLocalPosition = _gripPositionDefault.localPosition;
    }

    void Update() {

        // empty handed
        if (IsEmptyHanded) {
            if (m_InputHandler.GetGripInputDown()) { TryForwardGrab(); }
        }

        // gripped something
        else {
            if (m_InputHandler.GetGripInputDown()) {

                // throw it
                if (CanThrow()) { ThrowGrippedThing(); }

                // drop it
                else { SetDownGrippedThing(); }
            }

            // use it
            else { UpdateUseInput(m_InputHandler.GetAimInputDown(), m_InputHandler.GetAimInputHeld()); }
        }

        // swing
        if (CanPunch() && m_InputHandler.GetPunchInputDown()) {
            BeginPunch();
        }
    }

    void TryForwardGrab() {
        // search for grabbable thing
        var grippableThing = FindGrippableInGrabArea();
        if (grippableThing != null) {
            Grip(grippableThing);
        } else {
            TryPressButtonInGrabArea();
        }
    }

    // Update various animated features in LateUpdate because it needs to override the animated arm position
    void LateUpdate() {
        // TODO -- movement bob
        UpdateGripRecoil();

        // Set final weapon socket position based on all the combined animation influences
        _gripSocket.localPosition = _mainSocketLocalPosition + _mainSocketBobLocalPosition + _mainSocketRecoilLocalPosition;
    }

    void RecoilFist(float recoilForce) {
        m_AccumulatedRecoil += Vector3.back * recoilForce;
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

    void UpdateUseInput(bool useButtonDown, bool useButtonHeld) {
        // potion, etc
        if (_currentlyGrippedThing.useType == Grippable.UseType.Consumable ||
            _currentlyGrippedThing.useType == Grippable.UseType.Reusable) {
            if (useButtonDown) {
                BeginUseConsumable();
            }
        }

        // shield, etc
        else if (_currentlyGrippedThing.useType == Grippable.UseType.Hold) {
            // can start using if ready
            if (_gripMotionState == GripMotionState.Ready && useButtonDown) {
                BeginUseHold();
            }

            // can stop using if lifting up
            else if (_gripMotionState == GripMotionState.LiftingUp && !useButtonHeld) {
                EndUseHold();
            }
        }
    }

    bool CanThrow() {
        return _gripMotionState == GripMotionState.Ready && !_playerController.isCrouching;
    }

    bool CanPunch() {
        return _gripMotionState == GripMotionState.Ready;
    }

    void BeginPunch() {
        _gripMotionState = GripMotionState.InMotion;
        _playerWeaponsManager.LowerWeapon();
        ForwardPunch();
        PunchSequence();
    }

    void EndPunch() {
        _gripMotionState = GripMotionState.Ready;
        _playerWeaponsManager.RaiseWeapon();

        // can't aim while punching or empty handed
        if (IsEmptyHanded) {
            _playerWeaponsManager.SetAimBlock(false);
        }
    }

    void EndThrow() {
        _gripMotionState = GripMotionState.Ready;
        _playerWeaponsManager.SetAimBlock(false);
    }

    void TryPressButtonInGrabArea() {
        var button = SearchRaycast<PunchButton>();
        if (button != null) {
            button.PressButton();
        }
    }

    Grippable FindGrippableInGrabArea() {
        return SearchRaycast<Grippable>();
    }

    T SearchRaycast<T>() where T : MonoBehaviour {
        var lookOrigin = _playerWeaponsManager.lookPosition;
        var lookDirection = _playerWeaponsManager.shotDirection;

        // highest priority, item is directly under the crosshairs
        if (Physics.Raycast(lookOrigin, lookDirection,
                out RaycastHit rayHit, _gripRange, _gripLayer)) {

            var raySearch = rayHit.collider.gameObject.GetComponent<T>();
            if (raySearch != null) {
                return raySearch;
            }
        }

        // spherecast shouldn't reach behind player, offset it forward by its radius
        var sphereCenter = lookOrigin + lookDirection * _gripRadius;

        // sphere check shouldn't extend further than the regular ray check
        var sphereDistance = Mathf.Max(_gripRange - _gripRadius, 0);

        // if nothing is raycast, check a small distance on all sides for a nearby item
        if (Physics.SphereCast(sphereCenter, _gripRadius, lookDirection, out RaycastHit sphereHit, sphereDistance, _gripLayer)) {

            var sphereSearch = sphereHit.collider.gameObject.GetComponent<T>();
            if (sphereSearch != null) {
                return sphereSearch;
            }
        }

        return null;
    }

    void ForwardPunch() {
        var buttonTarget = SearchRaycast<PunchButton>();
        if (buttonTarget != null) {
            buttonTarget.PressButton();
        }
    }

    void Grip(Grippable gripped) {
        gripped.transform.position = _gripSocket.position;
        gripped.transform.rotation = _gripSocket.rotation;

        // get the offset vector from the body socket to the barrel's socket
        var socketOffset = _gripSocket.position - gripped.gripPoint.position;

        // shift barrel by that amount to line up
        gripped.transform.position += socketOffset;

        // parent the socket and change its old reference
        gripped.transform.SetParent(_gripSocket.transform, true);

        // make it official
        _currentlyGrippedThing = gripped;

        // set render layer index
        gripped.SetLayerMask(_playerWeaponsManager.GetWeaponLayerIndex());

        // gripped thing reacts to this
        gripped.BecomeGripped(_playerController.gameObject);

        // play a short pickup motion
        TeleportThenMoveFist(_gripPositionDown, _gripPositionDefault, .1f);

        // prevent player from using ADS while gripping
        _playerWeaponsManager.SetAimBlock(true);
    }

    // BUG: throwing forward often stops forward motion due to hitting the collider
    void ThrowGrippedThing() {
        _gripMotionState = GripMotionState.InMotion;
        _playerWeaponsManager.SetAimBlock(false);

        DoThrow();

        ThrowSequence();
    }

    void DoThrow() {
        var gripped = UnGrip();
        if (gripped != null) {
            gripped.ThrowFrom(_gripThrowOrigin.position, _playerWeaponsManager.shotDirection, _throwForce);
        } else {
            Debug.Log("Trying to throw empty handed");
        }
    }

    void SetDownGrippedThing() {
        // play a short pickup motion
        TeleportThenMoveFist(_gripPositionDown, _gripPositionDefault, .1f)
            .OnComplete(() => UnGrip());
    }

    Grippable UnGrip() {
        var ungrippedThing = _currentlyGrippedThing;

        // clear parent transforms and clear data
        _currentlyGrippedThing.transform.SetParent(null);
        _currentlyGrippedThing.UnGrip();
        _currentlyGrippedThing = null;

        return ungrippedThing;
    }

    // Tweening stuff
    void BeginUseHold() {
        // feels more responsive to begin immediately
        _gripMotionState = GripMotionState.InMotion;
        _currentlyGrippedThing.BeginUseHold();
        MoveFistTo(_gripPositionUseHold, _punchTime)
            .OnComplete(() => {
                _gripMotionState = GripMotionState.LiftingUp;
            });
    }

    void EndUseHold() {
        _gripMotionState = GripMotionState.InMotion;
        MoveFistTo(_gripPositionDefault, _cooldownTime)
            .OnComplete(() => {
                _currentlyGrippedThing.EndUseHold();
                _gripMotionState = GripMotionState.Ready;
            });
    }

    void BeginUseConsumable() {
        _gripMotionState = GripMotionState.InMotion;
        MoveFistTo(_gripPositionUseHold, _useTime / 2)
            .OnComplete(() => DoUseConsumable());
    }

    void DoUseConsumable() {
        _currentlyGrippedThing.UseItem();

        MoveFistTo(_gripPositionUseTwist, _useTime / 2)
            .OnComplete(() => EndUseConsumable());
    }

    void EndUseConsumable() {
        //_currentlyGrippedThing.UseItem();

        if (_currentlyGrippedThing.useType != Grippable.UseType.Reusable) {
            Destroy(_currentlyGrippedThing.gameObject);
            _currentlyGrippedThing = null;
            _playerWeaponsManager.SetAimBlock(false);
        }

        MoveFistTo(_gripPositionDefault, _cooldownTime);

        _gripMotionState = GripMotionState.Ready;
    }

    void PunchSequence() {
        DOTween.Sequence()
            .Append(MoveFistTo(_gripPositionPunchFinish, _punchTime, _attackCurve)) // punching
            .Append(MoveFistTo(_gripPositionDefault, _cooldownTime)) // recovering
            .OnComplete(() => EndPunch()); // cooldown finished
    }

    void ThrowSequence() {
        DOTween.Sequence()
            .Append(MoveFistTo(_gripPositionThrow, _throwTime, _attackCurve)) // throw
            .Append(MoveFistTo(_gripPositionDefault, _cooldownTime)) // recover
            .OnComplete(() => EndThrow());
    }

    // Tweening helpers
    Tween MoveFistTo(Transform goal, float duration) {
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

    Tween MoveFistTo(Transform target, float duration, AnimationCurve curve) {
        return MoveFistTo(target, duration).SetEase(curve);
    }

    Tween TeleportThenMoveFist(Transform snapTo, Transform moveTo, float duration) {
        // teleport
        _mainSocketLocalPosition = snapTo.localPosition;
        _gripSocket.localEulerAngles = snapTo.localEulerAngles;

        // move
        return MoveFistTo(moveTo, duration);
    }

    Tween TeleportThenMoveFist(Transform snapTo, Transform moveTo, float duration, AnimationCurve curve) {
        return TeleportThenMoveFist(snapTo, moveTo, duration).SetEase(curve);
    }
}