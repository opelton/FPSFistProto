﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

// unlocks a door, and can also help with locating the door
public class Keycard : MonoBehaviour {
    public Vector3Event onButtonPressed;
    public UnityEvent onUnlockProximity;
    [SerializeField] Transform _unlockPoint;
    [SerializeField] float _unlockDistance = 1f;
    [SerializeField] Transform _buttonTransform;
    [SerializeField] Renderer _lightRenderer;
    [SerializeField] Transform _buttonUpPosition;
    [SerializeField] Transform _buttonDownPosition;
    [SerializeField] float _buttonTravelTime;
    [SerializeField] Ease _buttonEase = Ease.InCubic;
    [SerializeField, ColorUsage(false, true)] Color _pressedLightColor = Color.white;
    [SerializeField, ColorUsage(false, true)] Color _readyLightColor = Color.grey;
    [SerializeField] AudioClip _buttonPressClip;
    AudioSource _audioSource;
    bool _buttonReady = true;
    bool _proximityTriggered = false;

    void OnEnable() {
        _lightRenderer.material.color = _buttonReady ? _readyLightColor : _pressedLightColor;
    }

    void Awake() {
        gameObject.GetComponent<Grippable>().onActivatedBegin.AddListener(TryPushButton);
        _audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate() {
        CheckProximityTrigger();
    }

    void CheckProximityTrigger() {
        if (!_proximityTriggered && Vector3.Distance(_unlockPoint.position, transform.position) <= _unlockDistance) {
            onUnlockProximity.Invoke();
            _proximityTriggered = true;
        }
    }

    public void TryPushButton(GameObject gripper) {
        if (_buttonReady) {
            ButtonDown();
        }
    }

    void ButtonDown() {
        _buttonReady = false;
        _lightRenderer.material.color = _pressedLightColor;

        _buttonTransform.DOLocalMove(_buttonDownPosition.localPosition, _buttonTravelTime)
            .SetEase(_buttonEase)
            .OnComplete(DoButtonPress);
    }

    void DoButtonPress() {
        _audioSource.PlayOneShot(_buttonPressClip);
        onButtonPressed.Invoke(transform.position);
        ButtonUp();
    }

    void ButtonUp() {
        _buttonTransform.DOLocalMove(_buttonUpPosition.localPosition, _buttonTravelTime)
            .SetEase(_buttonEase)
            .OnComplete(ReadyButton);
    }

    void ReadyButton() {
        _lightRenderer.material.color = _readyLightColor;
        _buttonReady = true;
    }
}