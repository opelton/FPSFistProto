using System;
using DG.Tweening;
using UnityEngine;

public class GripMelee : MonoBehaviour {
    public delegate Tween FistMotionDelegate(Transform goal, float duration);
    public delegate Tween ResetFistPositionDelegate(float duration);
    [SerializeField] Transform startPosition;
    [SerializeField] Transform endPosition;
    [SerializeField] Ease attackEasing = Ease.Linear;
    [SerializeField] float windupTime;
    [SerializeField] float strikeTime;
    [SerializeField] float cooldownTime;
    public void MeleeSequence(FistMotionDelegate fistMover, ResetFistPositionDelegate fistReset, Action beginMelee, Action executeMelee, Action endMelee) {
        DOTween.Sequence()
            .AppendCallback(beginMelee.Invoke)
            .Append(fistMover(startPosition, windupTime))
            .AppendCallback(executeMelee.Invoke)
            .Append(fistMover(endPosition, strikeTime).SetEase(attackEasing))
            .AppendCallback(endMelee.Invoke)
            .OnComplete(() => fistReset(cooldownTime));
    }
}