// // Old Skull Games
// // Bernard Barthelemy
// // 18:27

using System;
using System.Collections.Generic;
using OSG.Core;
using UnityEngine;

namespace OSG.Services
{
    /// <summary>
    /// Container defining all services we want to add dynamically at application start
    /// </summary>
    public class DynamicServicesAsset : ScriptableObject
    {
        [Serializable]
        private struct ServicesForPlatform
        {
            public RuntimePlatform platform;
            public ServiceBinding[] bindings;
        }
        
        [SerializeField] private ServiceBinding[] servicesForAllPlatforms;
        [SerializeField] private ServicesForPlatform[] servicesForPlatforms;

        public void FixBindings()
        {
            ValidateBindings(ref servicesForAllPlatforms);
            for (var index = 0; index < servicesForPlatforms.Length; index++)
            {
                ValidateBindings(ref servicesForPlatforms[index].bindings);
            }
        }

        private void ValidateBindings(ref ServiceBinding[] serviceBindings)
        {

            if(serviceBindings == null)
            {
                return;
            }

            for (int i = 0; i < serviceBindings.Length; ++i)
            {
                for (int j = i+1; j < serviceBindings.Length; ++j)
                {
                    if(serviceBindings[i].serviceInterface.baseType == serviceBindings[j].serviceInterface.baseType)
                    {
                        serviceBindings = FixBindings(serviceBindings.ToList());
                        return;
                    }
                }
            }
        }

        private ServiceBinding[] FixBindings(List<ServiceBinding> serviceBindings)
        {
            if (serviceBindings.Count > 0)
            {
                for (int i = 0; i < serviceBindings.Count; ++i)
                {
                    var interfaceType = serviceBindings[i].serviceInterface.baseType;
                    if(interfaceType==null)
                        continue;
                    for (int j = i + 1; j < serviceBindings.Count;)
                    {
                        if (serviceBindings[j].serviceInterface.baseType == interfaceType)
                        {
                            Debug.LogError($"Removing duplicated interface {interfaceType.Name} {i} {j}");
                            serviceBindings.RemoveAt(j);
                        }
                        else
                        {
                            ++j;
                        }
                    }
                }
            }

            return serviceBindings.ToArray();
        }

        public void GetServiceBindings(List<ServiceBinding> list)
        {
            var currentPlatform = Application.platform;
            list.AddRange(servicesForAllPlatforms);
            foreach (var forPlatform in servicesForPlatforms)
            {
                if (forPlatform.platform == currentPlatform)
                {
                    list.AddRange(forPlatform.bindings);
                }
            }
        }

#if UNITY_EDITOR
        public void GetEditorServiceBindings(List<ServiceBinding> list)
        {
            var currentPlatform = Application.platform;
            foreach (var forPlatform in servicesForPlatforms)
            {
                if (forPlatform.platform == RuntimePlatform.WindowsEditor)
                {
                    list.AddRange(forPlatform.bindings);
                }
            }
        }
#endif
        public bool AddBinding(Type instanceType, Type interfaceType)
        {

            ServiceBinding sb = new ServiceBinding(instanceType, interfaceType);

            if (servicesForAllPlatforms == null)
            {
                servicesForAllPlatforms = new[]{sb};
                return true;
            }

            foreach (ServiceBinding binding in servicesForAllPlatforms)
            {
                if(binding.serviceInterface == interfaceType)
                {
                    Debug.LogError($"{interfaceType.Name} already exists");
                    return false;
                }
            }
            var l = new List<ServiceBinding>(servicesForAllPlatforms) {sb};
            servicesForAllPlatforms = l.ToArray();
            return true;
        }
    }

}