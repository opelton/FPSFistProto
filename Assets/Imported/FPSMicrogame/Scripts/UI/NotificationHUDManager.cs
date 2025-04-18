﻿using UnityEngine;

public class NotificationHUDManager : MonoBehaviour {
    [Tooltip("UI panel containing the layoutGroup for displaying notifications")]
    public RectTransform notificationPanel;
    [Tooltip("Prefab for the notifications")]
    public NotificationToast notificationPrefab;

    void Awake() {
        PlayerWeaponsManager playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, NotificationHUDManager>(playerWeaponsManager, this);
        playerWeaponsManager.onAddedWeapon += OnPickupWeapon;
    }

    void OnUpdateObjective(UnityActionUpdateObjective updateObjective) {
        if (!string.IsNullOrEmpty(updateObjective.notificationText))
            CreateNotification(updateObjective.notificationText);
    }

    void OnPickupWeapon(WeaponController weaponController, int index) {
        if (index != 0)
            CreateNotification("Picked up weapon : " + weaponController.weaponName);
    }

    public void CreateNotification(string text) {
        NotificationToast notificationInstance = Instantiate(notificationPrefab, notificationPanel);
        notificationInstance.transform.SetSiblingIndex(0);
        
        notificationInstance.Initialize(text);
    }

    public void RegisterObjective(Objective objective) {
        objective.onUpdateObjective += OnUpdateObjective;
    }

    public void UnregisterObjective(Objective objective) {
        objective.onUpdateObjective -= OnUpdateObjective;
    }
}