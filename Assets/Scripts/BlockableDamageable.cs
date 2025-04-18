using UnityEngine;

// a held item that can block damage from reaching the player
public class BlockableDamageable : Damageable {
    [SerializeField] float blockAngle = 40f;
    public FloatEvent onDamageBlocked;

    bool _isBlocking = false;
    Transform _cameraTransform;

    protected override void Awake() {
        base.Awake();
        _cameraTransform = Camera.main.transform;
    }

    public override void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource, Vector3 damagePoint) {
        if (_isBlocking) {
            Vector3 toDamage = (damagePoint - _cameraTransform.position).normalized;
            float interceptAngle = Vector3.Angle(_cameraTransform.forward, toDamage);
            //Debug.Log(string.Format("Angle to damage: {0}\nIntercept Angle: {1}", toDamage, interceptAngle));

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