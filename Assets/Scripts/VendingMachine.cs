using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns items, currently has no limit, playtesters love this more than anything else in the whole prototype
public class VendingMachine : MonoBehaviour {
    [SerializeField] GameObject _vendPrefab;
    [SerializeField] float _vendCooldown;
    [SerializeField] Transform _vendPosition;
    float _nextVendTime = 0;

    public void VendItem() {
        var currentTime = Time.time;
        
        if (currentTime >= _nextVendTime) {
            Instantiate(_vendPrefab, _vendPosition.position, _vendPosition.rotation);
            _nextVendTime = currentTime + _vendCooldown;
        }
    }
}