using System;

// neat events that self-manage their listeners per-type
// was fun, but not as powerful as built-in UnityEvents, so I stopped using this
public abstract class StaticEvent<T> where T : StaticEvent<T> {
    public string Description;

    public delegate void EventListener(T eventInfo);
    static event EventListener _staticEventListeners;

    public static void RegisterListener(EventListener listener) { _staticEventListeners += listener; }
    public static void UnregisterListener(EventListener listener) { _staticEventListeners -= listener; }

    public void Dispatch() {
        if (_staticEventListeners != null) {
            _staticEventListeners(this as T);
        }
    }
}

// sample usage
/*
public class PlayerDamagedEvent : StaticEvent<PlayerDamagedEvent> { 
    public int damageAmount;
}

PlayerDamagedEvent.RegisterListener(foo);

PlayerDamagedEvent PDE;
PDE.damageAmount = 100;
PlayerDamagedEvent.Dispatch();
*/