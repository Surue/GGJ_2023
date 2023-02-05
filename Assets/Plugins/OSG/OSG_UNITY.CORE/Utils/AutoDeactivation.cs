using UnityEngine;

namespace OSG
{
    class AutoDeactivation : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
