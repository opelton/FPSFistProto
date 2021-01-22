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
    bool _usable = true;
    bool _isOpen = false;

    public void TryOpenDoor() {
        if (_usable) {
            _isOpen = true;
            BeginDoorMove();

            MoveDoorOpen().OnComplete(EndDoorMove);
        }
    }

    public void TryCloseDoor() {
        if (_usable) {
            _isOpen = false;
            BeginDoorMove();

            MoveDoorClosed().OnComplete(EndDoorMove);
        }
    }

    public void TryToggleDoor() {
        if(_isOpen) { 
            TryCloseDoor();            
        }
        else { 
            TryOpenDoor();
        }
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