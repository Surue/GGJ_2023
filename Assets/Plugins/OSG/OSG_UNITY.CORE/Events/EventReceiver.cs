using UnityEngine;

namespace OSG.EventSystem
{
    /// <summary>
    /// Allows to react on Game Events, in a way defined in the unity Inspector
    /// </summary>
    public class EventReceiver : OSGMono
    {
        [SerializeField]
        private EventTarget[] targets;

        void OnEnable()
        {
            if (targets == null) return;
            for (int i = 0; i < targets.Length; ++i)
            {
                if(!targets[i].Register())
                {
                    Debug.LogError($"target {i} failed to register in scene {gameObject.scene.name}");
                }
            }
        }

        void OnDisable()
        {
            if (targets == null) return;
            for (int i = 0; i < targets.Length; ++i)
            {
                targets[i].Unregister();
            }
        }
    }

}