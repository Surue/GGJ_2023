// Old Skull Games
// Bernard Barthelemy
// Friday, June 8, 2018

using System.Linq;
using UnityEngine;

namespace OSG
{
    class PlatformActivation : MonoBehaviour
    {
        [SerializeField] private RuntimePlatform[] platforms;
        [SerializeField] private bool activate;

        private void Awake()
        {
            bool thisPlatformIsInList = platforms != null && platforms.Contains(Application.platform);
            gameObject.SetActive(activate ? thisPlatformIsInList : !thisPlatformIsInList);
        }
    }
}
