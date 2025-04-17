using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// fires a gun in a straight line when activated, compatible with FPSMicrogame guns
public class MountedWeapon : MonoBehaviour {
    [SerializeField] WeaponController _weaponPrefab;
    [SerializeField] Transform _weaponRoot;
    [SerializeField] bool _fireOnAwake = true;
    WeaponController _mountedWeapon;
    bool _firing;

    void Awake() {
        _firing = _fireOnAwake;
        MountWeapon();
    }

    void Update() {
        if (_firing && !IsReloading()) {
            Autofire();
        }
    }

    bool IsReloading() {
        if (_mountedWeapon != null)
            return _mountedWeapon.isCooling;
        return true;
    }

    void MountWeapon() {
        _mountedWeapon = Instantiate(_weaponPrefab, _weaponRoot);
        _mountedWeapon.transform.localPosition = Vector3.zero;
        _mountedWeapon.transform.localRotation = Quaternion.identity;

        // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
        _mountedWeapon.owner = gameObject;
        _mountedWeapon.sourcePrefab = _weaponPrefab.gameObject;
        _mountedWeapon.ShowWeapon(true);

        // binds weaponcontroller's forward direction to the weapon shot angle
        //weaponInstance.OverloadShotAngle(weaponShotAngle);
    }

    void Autofire() {
        _mountedWeapon.HandleShootInputs(true, true, true);
    }

    public void ToggleAutofire() { _firing = !_firing; }
    public void AutofireOn() { _firing = true; }
    public void AutofireOff() { _firing = false; }
}