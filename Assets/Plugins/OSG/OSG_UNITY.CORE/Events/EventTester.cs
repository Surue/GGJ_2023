using UnityEngine;

namespace OSG.EventSystem
{
    public class EventTester : MonoBehaviour
    {
        public void IntFunction (int intParam)
        {
            UnityEngine.Debug.Log("Received int " + intParam);
        }
    }
}