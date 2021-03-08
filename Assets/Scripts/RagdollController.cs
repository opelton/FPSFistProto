using System.Linq;
using UnityEngine;
public class RagdollController : MonoBehaviour {
    Rigidbody _rootRb;
    Collider[] _ragdollParts;
    Animator _animator;
    Health _hp;
    Collider _groundCollider;

    void Awake() {
        _rootRb = GetComponent<Rigidbody>();
        _groundCollider = GetComponent<Collider>();
        _ragdollParts = GetComponentsInChildren<Collider>(true).Where(rb => rb != _rootRb).ToArray();
        _animator = GetComponent<Animator>();
        _hp = GetComponentInParent<Health>();
    }

    void Start() {
        _hp.onDie += HandleDeath;
        SetRagdoll(false);
    }

    void OnDestroy() {
        _hp.onDie -= HandleDeath;
    }

    void SetRagdoll(bool isRagdoll) {
        foreach (Collider rb in _ragdollParts) {
            rb.enabled = isRagdoll;
        }
        _groundCollider.enabled = !isRagdoll;
        _rootRb.useGravity = !isRagdoll;
        _rootRb.isKinematic = isRagdoll;
        _animator.enabled = !isRagdoll;
    }

    void HandleDeath() {
        SetRagdoll(true);
    }
}