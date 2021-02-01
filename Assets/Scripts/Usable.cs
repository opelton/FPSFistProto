using System;
using UnityEngine;
using UnityEngine.Events;

public class Usable : MonoBehaviour {
    public UnityEvent onUsed;

    public void Use() {
        onUsed.Invoke();
    }
}