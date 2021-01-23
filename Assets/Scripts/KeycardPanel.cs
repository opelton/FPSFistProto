using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class KeycardPanel : MonoBehaviour {
    public UnityEvent onDoorToggle;
    [SerializeField] float _unlockDistance = 10f;
    [SerializeField] AudioClip _pingSfx;
    [SerializeField] float _pingDelay = .25f;
    [SerializeField, ColorUsage(false, true)] Color _readyColor = Color.grey;
    [SerializeField, ColorUsage(false, true)] Color _pingColor = Color.white;
    [SerializeField, ColorUsage(false, true)] Color _keyColor = Color.green;
    [SerializeField] Renderer _pingBulb;
    [SerializeField] Renderer _keyBulb;
    AudioSource _audioSource;
    bool _panelReady = true;

    void Awake() {
        _audioSource = GetComponent<AudioSource>();
    }

    public void KeycardPing(Vector3 keycardPos) {
        var distance = (keycardPos - transform.position).magnitude;

        if (distance <= _unlockDistance) {
            KeyLightOn();
        }
        else { 
            PingLightOn();
        }

        if (_panelReady) {
            _panelReady = false;
            StartCoroutine(DelayedPing(distance));
        }
    }

    IEnumerator DelayedPing(float distance) {
        yield return new WaitForSeconds(_pingDelay);
        _audioSource.PlayOneShot(_pingSfx);
        _panelReady = true;

        if (distance <= _unlockDistance) {
            onDoorToggle.Invoke();
        }

        PingLightOff();
        KeyLightOff();
    }

    void PingLightOn() {
        _pingBulb.material.color = _pingColor;
    }

    void PingLightOff() {
        _pingBulb.material.color = _readyColor;
    }

    void KeyLightOn() {
        _keyBulb.material.color = _keyColor;
    }

    void KeyLightOff() {
        _keyBulb.material.color = _readyColor;
    }
}