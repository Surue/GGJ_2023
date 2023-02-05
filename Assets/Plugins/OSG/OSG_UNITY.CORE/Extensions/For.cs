// Old Skull Games
// Bernard Barthelemy
// Monday, August 28, 2017

using System;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Allows to iterate on ALL (active or not) Monobehaviours of type T
    /// in all currently loaded scenes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ForEach<T> where T : UnityEngine.Object
    {
        readonly T[] array = Resources.FindObjectsOfTypeAll<T>();

        private ForEach()
        {
            array = Resources.FindObjectsOfTypeAll<T>();
        }

        public static ForEach<T> Do(Action<T> action)
        {
            var  all = new ForEach<T>();
            var array = all.array;
            for (int i = all.array.Length; --i >= 0;)
            {
#if UNITY_EDITOR
                if (!array[i].IsSelectableSceneObject()) continue;
#endif
                action(array[i]);
            }
            return all;
        }
    }

    public class ForEach
    {
        public static void DoOnType(Type t, Action<UnityEngine.Object>action)
        {

            var all = Resources.FindObjectsOfTypeAll(t);
            for (int i = all.Length; --i >= 0;)
            {
#if UNITY_EDITOR
                if (!all[i].IsSelectableSceneObject()) continue;
#endif
                action(all[i]);
            }
        }
    }

}


/*
public static void Each<T>(Action<T> action) where T : MonoBehaviour
{
    var thingies = Resources.FindObjectsOfTypeAll(typeof(T));
    for (int i = thingies.Length; --i >= 0;)
    {
        T n = thingies[i] as T;
        if (n
#if UNITY_EDITOR
            && n.gameObject.IsSelectableSceneObject()
#endif
        )
        {
            action(n);
        }
    }
}*/
