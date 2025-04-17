using UnityEngine;

// shield item that blocks bullets when held and activated
[RequireComponent(typeof(Grippable))]
public class GripShield : MonoBehaviour {
    Grippable _gripBase;
    BlockableDamageable _blocker;

    void Awake() {
        _gripBase = GetComponent<Grippable>();
        _gripBase.onActivatedBegin.AddListener(BeginBlocking);
        _gripBase.onActivatedEnd.AddListener(EndBlocking);
        _gripBase.onGrabbed.AddListener(RegisterOwner);
        _gripBase.onDropped.AddListener(UnregisterOwner);
    }

    void RegisterOwner(GameObject owner) {
        _blocker = owner.GetComponent<BlockableDamageable>();
        _blocker.onDamageBlocked.AddListener(ShieldDamaged);
    }

    void UnregisterOwner() { 
        _blocker = null;
    }

    void ShieldDamaged(float impactForce) {
        _gripBase.onRecoilReceived.Invoke(impactForce);
    }

    void BeginBlocking(GameObject gripper) {
        _blocker.BeginBlocking();
    }

    void EndBlocking(GameObject gripper) {
        _blocker.EndBlocking();
    }
}