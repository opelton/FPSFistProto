using UnityEngine;

[RequireComponent(typeof(Grippable))]
[RequireComponent(typeof(Damageable))]
public class GripShield : MonoBehaviour {
    Grippable _gripBase;
    Damageable _damage;

    void Awake() {
        _gripBase = GetComponent<Grippable>();
        _gripBase.onActivatedBegin.AddListener(BeginBlocking);
        _gripBase.onActivatedEnd.AddListener(EndBlocking);

        _damage = GetComponent<Damageable>();
        _damage.onDamageTaken.AddListener(ShieldDamaged);
    }

    void ShieldDamaged(float impactForce) {
        _gripBase.onRecoilReceived.Invoke(impactForce);
    }

    void BeginBlocking(GameObject gripper) {
        _gripBase.EnableFirstPersonBlocking();
    }

    void EndBlocking(GameObject gripper) {
        _gripBase.DisableFirstPersonBlocking();
    }
}