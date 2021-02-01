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
    [SerializeField] Renderer _buttonRenderer;
    [SerializeField, ColorUsage(false, true)] Color _readyColor = Color.red;
    [SerializeField, ColorUsage(false, true)] Color _pressColor = Color.white;
    [SerializeField] Ease _pressEasing = Ease.Linear;
    [SerializeField] Ease _cooldownEasing = Ease.Linear;
    bool _buttonReady;

    void Awake() {
        _buttonReady = _readyOnAwake;
        _buttonRenderer.material.color = _buttonReady ? _readyColor : _pressColor;
    }

    public void PressButton() {
        if (_buttonReady) {
            _buttonReady = false;

            if (onButtonPressed != null) {
                onButtonPressed.Invoke();
            }

            SendButtonDown();
        }
    }

    public void PlayerUseButton(GameObject player) { 
        PressButton();
    }

    void SendButtonDown() {
        DOTween.Sequence()
            .Append(ChangeColorTo(_pressColor, _buttonPressDuration).SetEase(_pressEasing))
            .Join(MoveButtonTo(_buttonDownPosition, _buttonPressDuration).SetEase(_pressEasing))
            .OnComplete(BeginButtonCooldown);
    }

    void BeginButtonCooldown() {
        DOTween.Sequence()
            // .Append(ChangeColorTo(_readyColor, _buttonCooldown).SetEase(_cooldownEasing))
            // .Join(MoveButtonTo(_buttonUpPosition, _buttonCooldown).SetEase(_cooldownEasing))
            //.OnComplete(() => _buttonReady = true);
            .Append(MoveButtonTo(_buttonUpPosition, _buttonCooldown).SetEase(_cooldownEasing))
            .OnComplete(() => {
                _buttonReady = true;
                SetButtonColor(_readyColor);
            });
    }

    Tween ChangeColorTo(Color goal, float duration) {
        return _buttonRenderer.material.DOColor(goal, duration);
    }

    Tween MoveButtonTo(Transform goal, float duration) {
        return _buttonTransform.DOMove(goal.position, duration);
    }

    void SetButtonColor(Color color) {
        _buttonRenderer.material.color = color;
    }
}