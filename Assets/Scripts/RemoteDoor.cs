using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RemoteDoor : MonoBehaviour {
    [SerializeField] Transform _doorBarrier;
    [SerializeField] Transform _doorClosedPosition;
    [SerializeField] Transform _doorOpenPosition;
    [SerializeField] float _doorMoveTime = .5f;
    [SerializeField] Ease _doorEase = Ease.Linear;
    [SerializeField] bool _locked = false;
    bool _usable = true;
    bool _isOpen = false;

    public void TryOpenDoor() {
        if (!_locked) {
            _isOpen = true;
            BeginDoorMove();

            MoveDoorOpen().OnComplete(EndDoorMove);
        }
    }

    public void TryCloseDoor() {
        _isOpen = false;
        BeginDoorMove();

        MoveDoorClosed().OnComplete(EndDoorMove);
    }

    public void TryToggleDoor() {
        if (_isOpen) {
            TryCloseDoor();
        } else {
            TryOpenDoor();
        }
    }

    public void Unlock() { _locked = false; }
    public void Lock() { _locked = true; }

    public void TryUnlockOpen() {
        Unlock();
        TryOpenDoor();
    }

    Tween MoveDoorOpen() {
        return _doorBarrier.transform.DOLocalMove(_doorOpenPosition.localPosition, _doorMoveTime).SetEase(_doorEase);
    }

    Tween MoveDoorClosed() {
        return _doorBarrier.transform.DOLocalMove(_doorClosedPosition.localPosition, _doorMoveTime).SetEase(_doorEase);
    }

    void BeginDoorMove() {
        _usable = false;
    }

    void EndDoorMove() {
        _usable = true;
    }
}