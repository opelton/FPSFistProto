using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour {
    [Tooltip("Multiplier to apply to the received damage")]
    public float damageMultiplier = 1f;
    [Range(0, 1)]
    [Tooltip("Multiplier to apply to self damage")]
    public float sensibilityToSelfdamage = 0.5f;
    public Health health { get; private set; }
    public FloatEvent onDamageTaken;

    protected virtual void Awake() {
        // find the health component either at the same level, or higher in the hierarchy
        health = GetComponent<Health>();
        if (!health) {
            health = GetComponentInParent<Health>();
        }
    }

    public virtual void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource, Vector3 damagePoint) {
        var totalDamage = damage;

        // skip the crit multiplier if it's from an explosion
        if (!isExplosionDamage) {
            totalDamage *= damageMultiplier;
        }

        if (health) {
            // potentially reduce damages if inflicted by self
            if (health.gameObject == damageSource) {
                totalDamage *= sensibilityToSelfdamage;
            }

            health.TakeDamage(totalDamage, damageSource);
        }

        onDamageTaken.Invoke(damage);
    }
}