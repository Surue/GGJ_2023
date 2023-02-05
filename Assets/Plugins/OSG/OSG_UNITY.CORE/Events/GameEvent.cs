using System;
using OSG.Core.EventSystem;
using UnityEngine;

namespace OSG.EventSystem
{
    /// <summary>
    /// Defines the base type for a parameter-less GameEvent
    /// \details To make a derived type usable, you have to make it `[Serializable]`
    /// </summary>
    [Serializable] public class GameEvent : CoreEvent {}

    /// <summary>
    /// Base class for Game Events with a parameter of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable] public class GameEvent<T> : CoreEvent<T>{}

    [Serializable] public class UnityObjectEvent : GameEvent<UnityEngine.Object> { }
    [Serializable] public class GameObjectEvent : GameEvent<GameObject> { }
    [Serializable] public class TransformEvent : GameEvent<Transform> { }
    [Serializable] public class ColorEvent : GameEvent<Color> { }
    [Serializable] public class Vector2Event : GameEvent<Vector2> { }
    [Serializable] public class Vector3Event : GameEvent<Vector3> { }
    [Serializable] public class Vector4Event : GameEvent<Vector4> { }
    [Serializable] public class QuaternionEvent : GameEvent<Quaternion> { }
}
