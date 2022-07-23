using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SwitchPanel : MonoBehaviour {
    public UnityEvent onSwitchEnabled;
    public UnityEvent onSwitchDisabled;

    [SerializeField] bool startOn = false;
    [SerializeField] float switchTravelTime = .25f;
    [SerializeField] Transform switchTransform;
    [SerializeField] Transform onPosition;
    [SerializeField] Transform offPosition;
    [SerializeField] Renderer display;
    [SerializeField, ColorUsage(false, true)] Color onColor = Color.green;
    [SerializeField, ColorUsage(false, true)] Color offColor = Color.grey;
    bool _switchReady = true;
    bool _powered = false;

    void Start() {
        _powered = startOn;
        UpdateDisplay();
    }

    void UpdateDisplay() {
        display.material.color = _powered ? onColor : offColor;
    }

    public void OnUsed() {
        if (_switchReady) {
            _switchReady = false;
            if (_powered) {
                ButtonOffSequence();
            } else {
                ButtonOnSequence();
            }
        }
    }

    void ButtonOffSequence() {
        DOTween.Sequence()
            .Append(switchTransform.DOLocalMove(offPosition.localPosition, switchTravelTime))
            .AppendCallback(() => _powered = false)
            .AppendCallback(() => _switchReady = true)
            .AppendCallback(UpdateDisplay)
            .AppendCallback(onSwitchDisabled.Invoke);
    }

    void ButtonOnSequence() {
        DOTween.Sequence()
            .Append(switchTransform.DOLocalMove(onPosition.localPosition, switchTravelTime))
            .AppendCallback(() => _powered = true)
            .AppendCallback(() => _switchReady = true)
            .AppendCallback(UpdateDisplay)
            .AppendCallback(onSwitchEnabled.Invoke);
    }
}