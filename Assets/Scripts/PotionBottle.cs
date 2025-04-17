using UnityEngine;

// heals when used or smashed nearby
public class PotionBottle : MonoBehaviour {
    [SerializeField] GameObject onUsedFX;
    [SerializeField] float _healAmount = 10f;
    [SerializeField] float _splashRadius = 1f;
    Grippable _root;
    bool _destroying = false;

    void Start() {
        _root = GetComponent<Grippable>();
        _root.onActivatedBegin.AddListener(ConsumePotion);
        _root.onThrowImpact.AddListener(SmashPotion);

        // optionally, attacking the potion is the same as throwing it at the wall
        var health = GetComponent<Health>();
        if (health != null) {
            health.onDie += SmashPotion;
        }
    }

    public void ConsumePotion(GameObject target) {
        TryApplyPotionEffect(target);
        PotionVfx();
        DestroyPotion();
    }

    void SmashPotion() {
        // multiple sources can trigger this, make sure it only happens once
        if (!_destroying) {
            _destroying = true;

            var splashedTargets = PotionSplashTargets();
            foreach (var target in splashedTargets) {
                TryApplyPotionEffect(target.gameObject);
            }

            PotionVfx();
            DestroyPotion();
        }
    }

    Collider[] PotionSplashTargets() {
        return Physics.OverlapSphere(transform.position, _splashRadius);
    }

    void TryApplyPotionEffect(GameObject target) {
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