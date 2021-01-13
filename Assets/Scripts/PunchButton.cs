using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class PunchButton : MonoBehaviour {
    public UnityEvent onButtonPressed;
    [SerializeField] bool _readyOnAwake = true;
    [SerializeField] float _buttonCooldown = 1f;
    [SerializeField] float _buttonPressDuration = .1f;
    [SerializeField] Transform _buttonDownPosition;
    [SerializeField] Transform _buttonUpPosition;
    [SerializeField] Transform _buttonTransform;
    bool _buttonReady;

    void Awake() {
        _buttonReady = _readyOnAwake;
    }

    public void PressButton() {
        if (_buttonReady) {
            _buttonReady = false;

            if (onButtonPressed != null) {
                onButtonPressed.Invoke();
            }

            MoveButtonTo(_buttonDownPosition, _buttonPressDuration)
                .OnComplete(BeginButtonCooldown);
        }
    }

    void BeginButtonCooldown() {
        MoveButtonTo(_buttonUpPosition, _buttonCooldown)
            .OnComplete(() => _buttonReady = true);
    }

    Tween MoveButtonTo(Transform goal, float duration) {
        return _buttonTransform.DOMove(goal.position, duration);
    }
}