using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject> { }

public class Grippable : MonoBehaviour {
    public enum UseType { Hold, Consumable, Reusable }
    public UseType useType = UseType.Hold;
    [SerializeField] Transform gripAnchor;
    [SerializeField] float _throwDamage = 10f;
    public bool _breakable = false;
    [HideInInspector] public GameObjectEvent onUsed;
    [HideInInspector] public GameObjectEvent onUseHeldBegin;
    [HideInInspector] public GameObjectEvent onUseHeldEnd;
    [HideInInspector] public UnityEvent onSmashed;
    public Transform gripPoint { get { return gripAnchor != null ? gripAnchor : gameObject.transform; } }

    Collider _collider;
    Rigidbody _body;
    RigidbodyCopy _storedBody;
    int _storedLayerMask;
    bool _thrown;
    GameObject _gripper;

    void Start() {
        _collider = GetComponent<Collider>();
        _body = GetComponent<Rigidbody>();
        _storedLayerMask = gameObject.layer;
    }

    public void BecomeGripped(GameObject gripper) {
        _collider.enabled = false;

        // save a copy of the existing rigidbody and delete it
        // setting kinematic and disabling collisions DOES NOT disable it enough
        if (_body != null) {
            _storedBody = new RigidbodyCopy(_body);
            Destroy(_body);
            _body = null;
        }

        _gripper = gripper;
    }

    public void UnGrip() {
        _gripper = null;
        _collider.enabled = true;
        if (_storedBody != null) {
            _body = _storedBody.CopyTo(gameObject);
        }
        RestoreLayerMask();
    }

    public void Throw(Vector3 direction, float force) {
        _body.velocity = direction * force;
        _body.angularVelocity = transform.right * force;
        _thrown = true;
    }

    public void ThrowFrom(Vector3 position, Vector3 direction, float force) {
        transform.position = position;
        Throw(direction, force);
    }

    public void BeginUseHold() {
        onUseHeldBegin.Invoke(_gripper);
    }

    public void EndUseHold() {
        onUseHeldEnd.Invoke(_gripper);
    }

    void SmashItem() {
        onSmashed.Invoke();
        Destroy(gameObject);
    }

    public void UseItem() {
        if (onUsed != null) {
            onUsed.Invoke(_gripper);
        }
    }

    public void SetLayerMask(int layerIndex) {
        foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true)) {
            t.gameObject.layer = layerIndex;
        }
    }

    void RestoreLayerMask() {
        SetLayerMask(_storedLayerMask);
    }

    void OnCollisionEnter(Collision collision) {
        if (_thrown) {

            var checkRadius = _collider.bounds.size.magnitude * 1.2f;

            // button was nearby and in LOS to be effected
            var unobstructedButtons = CheckSphereAndLOS<PunchButton>(_collider.bounds.center, checkRadius);
            foreach (var button in unobstructedButtons) {
                button.PressButton();
            }

            // direct impact
            var damageable = collision.gameObject.GetComponent<Damageable>();
            if (damageable != null) {
                Debug.Log("Direct damage");
                damageable.InflictDamage(_throwDamage, false, _gripper, _collider.bounds.center);
            }
            // splash
            else {
                var visibleDamage = CheckSphereAndLOS<Damageable>(_collider.bounds.center, checkRadius);
                foreach (var dmg in visibleDamage) {
                    Debug.Log("Splash damage");
                    dmg.InflictDamage(_throwDamage, true, _gripper, _collider.bounds.center);
                }
            }

            if (_breakable) {
                SmashItem();
            }

            _thrown = false;
        }
    }

    // TODO -- some grippables hit buttons through trigger, and some through direct collision... hmmm....
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