using UnityEngine;

[RequireComponent(typeof(Grippable))]
public class GripShield : MonoBehaviour {
    [SerializeField] float _protectionAngleMinX = -30;
    [SerializeField] float _protectionAngleMaxX = 30;
    Grippable _gripBase;

    void Awake() {
        _gripBase = GetComponent<Grippable>();
        _gripBase.onUseHeldBegin.AddListener(BeginBlocking);
        _gripBase.onUseHeldEnd.AddListener(EndBlocking);
    }

    void BeginBlocking(GameObject gripper) {
        var dmg = gripper.GetComponent<Damageable>();
        Debug.Log(gripper.name);
        dmg._directionalBlock = true;
        dmg._directionalBlockAngleMax = _protectionAngleMaxX;
        dmg._directionalBlockAngleMin = _protectionAngleMinX;
    }

    void EndBlocking(GameObject gripper) {
        var dmg = gripper.GetComponent<Damageable>();
        dmg._directionalBlock = false;
        dmg._directionalBlockAngleMax = 0;
        dmg._directionalBlockAngleMin = 0;
    }
}