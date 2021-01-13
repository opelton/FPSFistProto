using UnityEngine;
using UnityEngine.Events;

public class Grippable : MonoBehaviour {
    public enum UseType { Hold, Consumable }
    public UseType useType = UseType.Hold;
    [SerializeField] Transform gripAnchor;
    public bool _breakable = false;
    [HideInInspector] public UnityEvent onUsed;
    public Transform gripPoint { get { return gripAnchor != null ? gripAnchor : gameObject.transform; } }

    Collider _collider;
    Rigidbody _body;
    RigidbodyCopy _storedBody;
    int _storedLayerMask;
    bool _thrown;

    void Start() {
        _collider = GetComponent<Collider>();
        _body = GetComponent<Rigidbody>();
        _storedLayerMask = gameObject.layer;
    }

    public void BecomeGripped() {
        _collider.enabled = false;

        // save a copy of the existing rigidbody and delete it
        // setting kinematic and disabling collisions DOES NOT disable it enough
        if (_body != null) {
            _storedBody = new RigidbodyCopy(_body);
            Destroy(_body);
            _body = null;
        }
    }

    public void UnGrip() {
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

    void SmashItem() {
        UseItem();
        Destroy(gameObject);
    }

    public void UseItem() {
        if (onUsed != null) {
            onUsed.Invoke();
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
        if(_thrown && _breakable) { 
            SmashItem();

            // var button = collision.gameObject.GetComponent<PunchButton>();
            // if(button != null) { 
            //     button.PressButton();
            // }
        }
    }

    void OnTriggerEnter(Collider other) { 
        if(_thrown) { 
            var button = other.gameObject.GetComponent<PunchButton>();
            if(button != null) { 
                button.PressButton();
            }
        }
    }
}