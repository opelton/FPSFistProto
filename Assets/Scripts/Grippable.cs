using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// item the player can pick up and hold in their off-hand
public class Grippable : MonoBehaviour {
    public enum ActivationType { Nothing, Press, Hold }

    [SerializeField] ActivationType _activationType = ActivationType.Press;
    [SerializeField] Transform gripAnchor;
    [SerializeField] float _throwDamage = 10f;

    [HideInInspector] public GripActionEvent onGrabbed;
    [HideInInspector] public UnityEvent onDropped;
    [HideInInspector] public GripActionEvent onThrown;
    [HideInInspector] public UnityEvent onThrowImpact;
    [HideInInspector] public GripActionEvent onActivatedBegin;
    [HideInInspector] public GripActionEvent onActivatedEnd;
    [HideInInspector] public FloatEvent onRecoilReceived;

    // members
    public Transform gripPoint => gripAnchor != null ? gripAnchor : gameObject.transform;
    public ActivationType activationType => _activationType;
    public GameObject GripOwner => _owner;

    // privates
    Collider _collider;
    Rigidbody _body;
    LayerMask _hudLayerMask;
    int _storedLayerMask;
    bool _thrown;
    bool _blocking;
    GameObject _owner;
    RigidbodyCopy _shelvedBody;

    void Start() {
        _collider = GetComponent<Collider>();
        _body = GetComponent<Rigidbody>();
        _storedLayerMask = gameObject.layer;
    }

    // moves a gripped item from worldspace to player's camera space, removing its collision, and rendering it over other scenery
    public void BecomeGripped(GameObject gripper, LayerMask hudLayer) {
        _hudLayerMask = hudLayer;
        OverrideLayerMask();

        ShelvePhysics();

        _owner = gripper;
        onGrabbed.Invoke(_owner);
    }

    public void UnGrip() {
        _owner = null;
        RestorePhysics();
        RestoreLayerMask();
        onDropped.Invoke();
    }

    public void Throw(Vector3 direction, float force) {
        ApplyForce(direction, force);

        // the spin looks neat for throwing stuff
        _body.angularVelocity = transform.right * force;
        _thrown = true;

        onThrown.Invoke(_owner);
    }

    public void ApplyForce(Vector3 direction, float force) {
        if (_body != null) {
            _body.velocity += direction * force;
        }
    }

    public void DestroyItem() {
        Destroy(gameObject);
    }

    public void ThrowImpact() {
        onThrowImpact.Invoke();
    }

    public void ActivateBegin() {
        onActivatedBegin.Invoke(_owner);
    }

    public void ActivateEnd() {
        onActivatedEnd.Invoke(_owner);
    }

    void SetLayerMask(int layerIndex) {
        foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true)) {
            t.gameObject.layer = layerIndex;
        }
    }

    void OverrideLayerMask() {
        SetLayerMask(_hudLayerMask);
    }

    void RestoreLayerMask() {
        SetLayerMask(_storedLayerMask);
    }

    void ShelvePhysics() {
        _collider.enabled = false;

        if (_body != null) {

            // only needs to happen once per item
            if (_shelvedBody == null) {
                _shelvedBody = new RigidbodyCopy(_body);
            }

            Destroy(_body);
            _body = null;
        }
    }

    void RestorePhysics() {
        _collider.enabled = true;

        if (_shelvedBody != null) {
            _body = _shelvedBody.CopyTo(gameObject);
        }
    }

    // TODO -- throw collisions are inconsistent
    void OnCollisionEnter(Collision collision) {
        if (_thrown) {
            // flip buttons on direct contact
            var button = collision.gameObject.GetComponent<PunchButton>();
            if (button != null) {
                button.PressButton();
            } else {
                // long items thrown directly at a button can hit the wall around it, do a sphere check to make it feel better
                var checkRadius = GetMaxBoundsLength(_collider.bounds);

                // button was nearby and in LOS to be effected
                var unobstructedButtons = CheckSphereAndLOS<PunchButton>(_collider.bounds.center, checkRadius);
                foreach (var losButton in unobstructedButtons) {
                    losButton.PressButton();
                }
            }

            // direct impact, but watch out for players throwing boxes into their own faces
            var damageable = collision.gameObject.GetComponent<Damageable>();
            if (damageable != null && collision.gameObject.GetComponent<PlayerCharacterController>() == null) {
                damageable.InflictDamage(_throwDamage, false, _owner, _collider.bounds.center);
            }

            ThrowImpact();

            _thrown = false;
        }
    }

    float GetMaxBoundsLength(Bounds bounds) {
        var largest = Mathf.Max(bounds.size.x, bounds.size.y);
        largest = Mathf.Max(largest, bounds.size.z);
        return largest;
    }

    void OnTriggerEnter(Collider other) {
        if (_thrown) {
            var button = other.gameObject.GetComponent<PunchButton>();
            if (button != null) {
                button.PressButton();
            }
        }
    }

    // returns objects that overlap the sphere and were in line of sight
    T[] CheckSphereAndLOS<T>(Vector3 center, float radius) where T : MonoBehaviour {
        var overlaps = Physics.OverlapSphere(center, radius);
        List<T> losVisible = new List<T>();

        foreach (Collider nearby in overlaps) {
            var targetComponent = nearby.gameObject.GetComponent<T>();
            if (targetComponent != null) {

                // check unobstructed LOS
                if (Physics.Raycast(center, targetComponent.transform.position - center, out RaycastHit hit)) {
                    var unobstructed = hit.collider.GetComponent<T>();

                    // add confirmed to 
                    if (unobstructed != null) {
                        losVisible.Add(targetComponent);
                    }
                }
            }
        }

        return losVisible.ToArray();
    }
}