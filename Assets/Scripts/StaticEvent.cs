using System;

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