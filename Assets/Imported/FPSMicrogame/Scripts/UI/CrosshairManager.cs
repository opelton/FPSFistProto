using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour {
    public Image crosshairImage;
    public Sprite nullCrosshairSprite;
    public float crosshairUpdateshrpness = 5f;

    PlayerWeaponsManager m_WeaponsManager;
    bool m_WasPointingAtEnemy;
    RectTransform m_CrosshairRectTransform;
    CrosshairData m_CrosshairDataDefault;
    CrosshairData m_CrosshairDataTarget;
    CrosshairData m_CurrentCrosshair;
    Camera _mainCamera;

    void Start() {
        _mainCamera = Camera.main;

        m_WeaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, CrosshairManager>(m_WeaponsManager, this);

        OnWeaponChanged(m_WeaponsManager.GetActiveWeapon());
        m_WeaponsManager.onSwitchedToWeapon += OnWeaponChanged;
    }

    void Update() {
        UpdateCrosshairPointingAtEnemy(false);
        m_WasPointingAtEnemy = m_WeaponsManager.isPointingAtEnemy;
    }

    void UpdateCrosshairPointingAtEnemy(bool force) {
        if (m_CrosshairDataDefault.crosshairSprite == null)
            return;

        if ((force || !m_WasPointingAtEnemy) && m_WeaponsManager.isPointingAtEnemy) {
            m_CurrentCrosshair = m_CrosshairDataTarget;
            crosshairImage.sprite = m_CurrentCrosshair.crosshairSprite;
            m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.crosshairSize * Vector2.one;
        } else if ((force || m_WasPointingAtEnemy) && !m_WeaponsManager.isPointingAtEnemy) {
            m_CurrentCrosshair = m_CrosshairDataDefault;
            crosshairImage.sprite = m_CurrentCrosshair.crosshairSprite;
            m_CrosshairRectTransform.sizeDelta = m_CurrentCrosshair.crosshairSize * Vector2.one;
        }

        crosshairImage.color = Color.Lerp(crosshairImage.color, m_CurrentCrosshair.crosshairColor, Time.deltaTime * crosshairUpdateshrpness);

        m_CrosshairRectTransform.sizeDelta = Mathf.Lerp(m_CrosshairRectTransform.sizeDelta.x,
            m_CurrentCrosshair.crosshairSize, Time.deltaTime * crosshairUpdateshrpness) * Vector2.one;
    }

    void UpdateCrosshairScreenPosition(Vector3 aimRayPoint) {
        m_CrosshairRectTransform.position = _mainCamera.WorldToScreenPoint(aimRayPoint);
    }

    void OnWeaponChanged(WeaponController newWeapon) {
        if (newWeapon) {
            crosshairImage.enabled = true;
            m_CrosshairDataDefault = newWeapon.crosshairDataDefault;
            m_CrosshairDataTarget = newWeapon.crosshairDataTargetInSight;
            m_CrosshairRectTransform = crosshairImage.GetComponent<RectTransform>();
            DebugUtility.HandleErrorIfNullGetComponent<RectTransform, CrosshairManager>(m_CrosshairRectTransform, this, crosshairImage.gameObject);
        } else {
            if (nullCrosshairSprite) {
                crosshairImage.sprite = nullCrosshairSprite;
            } else {
                crosshairImage.enabled = false;
            }
        }

        // make sure crosshairs are assigned
        if (m_CrosshairRectTransform == null) {
            m_CrosshairRectTransform = crosshairImage.GetComponent<RectTransform>();
        }

        // aim at a fixed location based on look position and shot direction (this differs from camera forward)
        var aimPoint = m_WeaponsManager.lookPosition + m_WeaponsManager.shotDirection * 100f;
        UpdateCrosshairScreenPosition(aimPoint);

        UpdateCrosshairPointingAtEnemy(true);
    }
}