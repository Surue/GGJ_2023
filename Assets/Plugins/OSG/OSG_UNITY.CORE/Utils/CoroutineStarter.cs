// Old Skull Games
// Antoine Pastor
// 12/07/2018

using System.Collections;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Used to start Coroutines on a cached DontDestroyOnLoad MonoBehaviour.
    /// For example when a non-MonoBehaviour object needs to start a Coroutine.
    /// </summary>
    public class CoroutineStarter : MonoBehaviour
    {
        /// <summary>
        /// Starts the given Coroutine on a cached DontDestroyOnLoad MonoBehaviour.
        /// </summary>
        /// <param name="coroutine"></param>
        public static Coroutine Start(IEnumerator coroutine)
        {
            var coroutineStarter = GetCachedCoroutineStarter();
            return coroutineStarter.StartInternal(coroutine);
        }

        /// <summary>
        /// Stops the given Coroutine if it was started by the CoroutineStarter.
        /// </summary>
        /// <param name="coroutine"></param>
        public static void Stop(Coroutine coroutine)
        {
            if (coroutine == null)
                return;

            var coroutineStarter = GetCachedCoroutineStarter();
            coroutineStarter.StopInternal(coroutine);
        }

        /// <summary>
        /// Returns the cached CoroutineStarter or creates a new one if there is nothing in the cache.
        /// </summary>
        /// <returns></returns>
        private static CoroutineStarter GetCachedCoroutineStarter()
        {
            if (!cachedCoroutineStarter)
            {
                GameObject go = new GameObject();
                DontDestroyOnLoad(go);
                cachedCoroutineStarter = go.AddComponent<CoroutineStarter>();
            }

            return cachedCoroutineStarter;
        }

        private Coroutine StartInternal(IEnumerator action)
        {
            return StartCoroutine(action);
        }

        private void StopInternal(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }


        private static CoroutineStarter cachedCoroutineStarter;
    }
}

