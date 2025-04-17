using System;
using UnityEngine;
using UnityEngine.Events;

// the player can press E on this
public class Usable : MonoBehaviour {
    public UnityEvent onUsed;

    public void Use() {
        onUsed.Invoke();
    }
}