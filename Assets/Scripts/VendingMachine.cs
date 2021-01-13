using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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