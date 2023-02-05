using System;
using System.Linq;
using UnityEngine;

namespace OSG
{
    public class FallbackComponent : MonoBehaviour
    {
        [SerializeField] Behaviour[] componentsToManage;

        void Update()
        {
            UpdateManagedComponents();
        }

        public void UpdateManagedComponents()
        {
            foreach (Behaviour behaviour in componentsToManage)
            {
                if (behaviour)
                    UpdateManagedComponent(behaviour);
            }
        }

        private void UpdateManagedComponent(Behaviour componentToManage)
        {
            Type componentType = componentToManage.GetType();
            var objects = FindObjectsOfType(componentType);
            int otherCount = objects.Count(o => o != componentToManage);

            if (otherCount == 0)
            {
                if (!componentToManage.enabled)
                {
                    componentToManage.enabled = true;
                    UnityEngine.Debug.Log("<b>No " + componentType.Name + " found, activating fallback " + componentToManage +
                              "</b>");
                }
            }
            else
            {
                if (componentToManage.enabled)
                {
                    componentToManage.enabled = false;
                    UnityEngine.Debug.Log("<b>Found " + otherCount + " " + componentType.Name + "</b>");
                } 
            }
            
        }
    }
}