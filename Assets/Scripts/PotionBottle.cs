using UnityEngine;

public class PotionBottle : MonoBehaviour {
    [SerializeField] GameObject onUsedFX;
    [SerializeField] float _healAmount = 10f;
    [SerializeField] float _splashRadius = 1f;
    Grippable _root;
    bool _smashing = false;

    void Start() {
        _root = GetComponent<Grippable>();
        _root.onActivatedBegin.AddListener(ConsumePotion);
        _root.onThrowImpact.AddListener(SmashPotion);

        GetComponent<Health>().onDie += SmashPotion;
    }

    public void ConsumePotion(GameObject gripper) {
        ApplyPotionEffect(gripper);
        PotionVfx();
        DestroyPotion();
    }

    void SmashPotion() {
        if(!_smashing) {
            _smashing = true;
            var splashedTargets = Physics.OverlapSphere(transform.position, _splashRadius);
            foreach (var target in splashedTargets) {
                ApplyPotionEffect(target.gameObject);
            }

            PotionVfx();
            DestroyPotion();
        }
    }

    void ApplyPotionEffect(GameObject target) {
        Health playerHealth = target.GetComponent<Health>();
        if (playerHealth && playerHealth.canPickup()) {
            playerHealth.Heal(_healAmount);
        }
    }

    void PotionVfx() {
        Instantiate(onUsedFX, transform.position, transform.rotation);
    }

    void DestroyPotion() {
        _root.DestroyItem();
    }
}