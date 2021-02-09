using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class GripActionEvent : UnityEvent<GameObject> { }

[Serializable]
public class FloatEvent : UnityEvent<float> { }

[Serializable]
public class Vector3Event : UnityEvent<Vector3> { }