using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

public class Damageable : MonoBehaviour {
    [Tooltip("Multiplier to apply to the received damage")]
    public float damageMultiplier = 1f;
    [Range(0, 1)]
    [Tooltip("Multiplier to apply to self damage")]
    public float sensibilityToSelfdamage = 0.5f;
    public bool _directionalBlock = false;
    public float _directionalBlockAngleMin = -45;
    public float _directionalBlockAngleMax = 45;

    public Health health { get; private set; }

    [HideInInspector] public FloatEvent onDamageBlocked;

    void Awake() {
        // find the health component either at the same level, or higher in the hierarchy
        health = GetComponent<Health>();
        if (!health) {
            health = GetComponentInParent<Health>();
        }
    }

    public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource, Vector3 damagePoint) {
        if (health) {
            var totalDamage = damage;

            // skip the crit multiplier if it's from an explosion
            if (!isExplosionDamage) {
                totalDamage *= damageMultiplier;
            }

            // potentially reduce damages if inflicted by self
            if (health.gameObject == damageSource) {
                totalDamage *= sensibilityToSelfdamage;
            }

            // damage is protected from certain angles
            if (_directionalBlock) {
                var damageVector = (damagePoint - transform.position).normalized;
                var damageVectorSquashed = damageVector;
                damageVectorSquashed.y = 0;
                var flatForward = transform.forward;
                flatForward.y = 0;

                var angle = Vector3.SignedAngle(flatForward, damageVectorSquashed, Vector3.up);
                //Debug.Log(string.Format("Angle: {0}", angle));

                if (angle >= _directionalBlockAngleMin && angle <= _directionalBlockAngleMax) {
                    // damage blocked
                    health.BlockDamage(totalDamage, damageSource, damagePoint);
                    onDamageBlocked.Invoke(totalDamage / 100); // TODO determine knockback force
                    //Debug.Log(string.Format("Blocked {0} damage from {1} at {2} degrees", totalDamage, damageSource.name, angle));

                } else {
                    // didn't block damage
                    health.TakeDamage(totalDamage, damageSource);
                }

            } else {
                // apply the damage without computing block angle
                health.TakeDamage(totalDamage, damageSource);
            }
        }
    }
}