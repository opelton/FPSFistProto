using UnityEngine;

public class PotionBottle : MonoBehaviour { 
    [SerializeField] GameObject onUsedFX;
    Grippable _root;

    void Start() {
        _root = GetComponent<Grippable>();
        _root.onUsed.AddListener(SplashPotion);
    }

    public void SplashPotion(GameObject gripper) { 
        Instantiate(onUsedFX, transform.position, transform.rotation);
    }
}