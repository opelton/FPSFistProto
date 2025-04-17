using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// unlocks a door when the associated keycard is nearby, emits a noise when the keycard pings the door's location
public class KeycardPanel : MonoBehaviour {
    public UnityEvent onDoorToggle;
    [SerializeField] float _unlockDistance = 10f;
    [SerializeField] AudioClip _pingSfx;
    [SerializeField] AudioClip _unlockSfx;
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

    // todo -- ping light turns on early, give it the same delay as the ping sound
    public void KeycardPing(Vector3 keycardPos) {
        var distance = (keycardPos - transform.position).magnitude;

        if (distance <= _unlockDistance) {
            KeyLightOn();
        } else {
            PingLightOn();
        }

        if (_panelReady) {
            _panelReady = false;
            StartCoroutine(DelayedPing(distance));
        }
    }

    public void KeycardUnlock() {
        _audioSource.spatialBlend = 0;
        _audioSource.pitch = 1.0f;
        _audioSource.PlayOneShot(_unlockSfx);
        onDoorToggle.Invoke();
    }

    IEnumerator DelayedPing(float distance) {
        yield return new WaitForSeconds(_pingDelay);
        _panelReady = true;
        _audioSource.spatialBlend = 0.85f;
        if (distance <= _unlockDistance) {
            // playtesters didn't get that they also had to push the keycard button when near the door, so the unlock is automatic now
            //onDoorToggle.Invoke();    
            _audioSource.pitch = 0.5f;
        } else {
            _audioSource.pitch = 1.0f;
        }
        _audioSource.PlayOneShot(_pingSfx);

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