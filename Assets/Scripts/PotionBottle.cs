using UnityEngine;

public class PotionBottle : MonoBehaviour {
    [SerializeField] GameObject onUsedFX;
    [SerializeField] float _healAmount = 10f;
    Grippable _root;

    void Start() {
        _root = GetComponent<Grippable>();
        _root.onUsed.AddListener(ConsumePotion);
        _root.onSmashed.AddListener(SplashPotion);
    }

    public void ConsumePotion(GameObject gripper) {
        Health playerHealth = gripper.GetComponent<Health>();
        if (playerHealth && playerHealth.canPickup()) {
            playerHealth.Heal(_healAmount);
        }
        SplashPotion();
    }

    public void SplashPotion() {
        Instantiate(onUsedFX, transform.position, transform.rotation);
    }
}