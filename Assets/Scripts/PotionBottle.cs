using UnityEngine;

public class PotionBottle : MonoBehaviour { 
    [SerializeField] GameObject onUsedFX;

    public void SplashPotion() { 
        Instantiate(onUsedFX, transform.position, transform.rotation);
    }
}