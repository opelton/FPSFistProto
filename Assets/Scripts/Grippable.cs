using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GripActionEvent : UnityEvent<GameObject> { }

public class Grippable : MonoBehaviour {
    public enum ActivationType { Press, Hold }

    [SerializeField] ActivationType _activationType = ActivationType.Press;
    [SerializeField] Transform gripAnchor;
    [SerializeField] float _throwDamage = 10f;

    [HideInInspector] public GripActionEvent onGrabbed;
    [HideInInspector] public GripActionEvent onDropped;
    [HideInInspector] public GripActionEvent onThrown;
    [HideInInspector] public UnityEvent onThrowImpact;
    [HideInInspector] public GripActionEvent onActivatedBegin;
    [HideInInspector] public GripActionEvent onActivatedEnd;

    // members
    public Transform gripPoint => gripAnchor != null ? gripAnchor : gameObject.transform;
    public ActivationType activationType => _activationType;

    // privates
    Collider _collider;
    Rigidbody _body;
    RigidbodyCopy _storedBody;
    LayerMask _hudLayerMask;
    int _storedLayerMask;
    bool _thrown;
    GameObject _owner;

    void Start() {
        _collider = GetComponent<Collider>();
        _body = GetComponent<Rigidbody>();
        _storedLayerMask = gameObject.layer;
    }

    public void BecomeGripped(GameObject gripper, LayerMask hudLayer) {
        _hudLayerMask = hudLayer;
        OverrideLayerMask();

        DeleteRigidbodies();

        _owner = gripper;
        onGrabbed.Invoke(_owner);
    }

    public void UnGrip() {
        _owner = null;
        RestoreRidigbodies();
        RestoreLayerMask();
    }

    public void Throw(Vector3 direction, float force) {
        ApplyForce(direction, force);

        // the spin looks neat for throwing stuff
        _body.angularVelocity = transform.right * force;
        _thrown = true;

        onThrown.Invoke(_owner);
    }

    public void ApplyForce(Vector3 direction, float force) {
        _body.velocity += direction * force;
    }

    public void ThrowFrom(Vector3 position, Vector3 direction, float force) {
        transform.position = position;
        Throw(direction, force);
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

    // setting kinematic and disabling collisions DOES NOT disable rigidbody enough
    void DeleteRigidbodies() {
        _collider.enabled = false;

        // save a copy of the existing rigidbody and delete it
        if (_body != null) {
            _storedBody = new RigidbodyCopy(_body);
            Destroy(_body);
            _body = null;
        }
    }

    void RestoreRidigbodies() {
        _collider.enabled = true;

        if (_storedBody != null) {
            _body = _storedBody.CopyTo(gameObject);
        }
    }

    // TODO -- some grippables hit buttons through trigger, and some through direct collision... hmmm....
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