using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Vector3Event : UnityEvent<Vector3> { }

public class Keycard : MonoBehaviour {
    public Vector3Event onButtonPressed;
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

    void OnEnable() {
        _lightRenderer.material.color = _buttonReady ? _readyLightColor : _pressedLightColor;
    }

    void Awake() {
        gameObject.GetComponent<Grippable>().onUsed.AddListener(TryPushButton);
        _audioSource = GetComponent<AudioSource>();
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