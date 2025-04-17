using UnityEngine;

// a held item that can block damage from reaching the player
public class BlockableDamageable : Damageable {
    [SerializeField] float blockAngle = 90f;
    [SerializeField] Transform forwardBlockTransform;

    public FloatEvent onDamageBlocked;

    // tweening positions
    public Vector3 BlockForward => forwardBlockTransform != null ? forwardBlockTransform.forward : transform.forward;
    public Vector3 BlockPosition => forwardBlockTransform != null ? forwardBlockTransform.position : transform.position;

    bool _isBlocking = false;

    public override void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource, Vector3 damagePoint) {
        if (_isBlocking) {
            var damageNormal = (damagePoint - BlockPosition).normalized;
            var interceptAngle = Vector3.Angle(BlockForward, damageNormal);
            Debug.Log(interceptAngle);
            if (interceptAngle <= blockAngle) {
                onDamageBlocked.Invoke(damage);
                return;
            }
        }
        base.InflictDamage(damage, isExplosionDamage, damageSource, damagePoint);
    }

    public void BeginBlocking() { _isBlocking = true; }
    public void EndBlocking() { _isBlocking = false; }
}