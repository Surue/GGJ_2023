// Old Skull Games
// Bernard Barthelemy
// Wednesday, October 16, 2019

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OSG.Services
{
    /// <summary>
    /// Dynamically creates and registers the service modules defined in
    /// the DynamicModulesAsset named DynamicModules in Resources folder
    /// </summary>
    public static class DynamicServiceManager
    {
        public static string AssetName = "DynamicServices";
        [EditorPrefs(onChangeCallback= nameof(ReportServicesAtStart))]
        private static bool reportServicesAtStart = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AppAwake()
        {
            Application.quitting += OnApplicationQuitting;
            AddDynamicServices();
        }

        private static void OnApplicationQuitting()
        {
            ServiceProvider.Dispose();
        }


        private static void AddDynamicServices()
        {
            var l = GetModuleTypes();
            if (l != null)
            {
                List<IService> services = new List<IService>(l.Count);

                foreach (var binding in l)
                {
                    services.Add(AddDynamicService(binding));
                }
                
                foreach (IService service in services)
                {
                    service?.OnAllServicesReady();
                }
            }
        }

#if UNITY_EDITOR
        public static void AddEditorDynamicServices()
        {
            AddDynamicServices();
        }
#endif

        private static List<ServiceBinding> GetModuleTypes()
        {
            var asset = Resources.Load<DynamicServicesAsset>(AssetName);
            if (!asset)
            {
                return null;
            }
            asset.FixBindings();

            List<ServiceBinding> list = new List<ServiceBinding>();

            if (Application.isPlaying)
            {
                asset.GetServiceBindings(list);
            }
#if UNITY_EDITOR
            else
            {
                asset.GetEditorServiceBindings(list);
            }
#endif

            return list;
        }

        private static IService AddDynamicService(ServiceBinding binding)
        {
            var implementationType = (Type)binding.serviceType;
            ConstructorInfo constructor = implementationType.GetConstructor(new Type[0]);
            if(constructor==null)
            {
                throw new Exception($"Could not get constructor for module type {implementationType.Name}");
            }

            var service = constructor.Invoke(new object[0]);
            if(service==null)
            {
                throw new Exception($"Could not create module {implementationType.Name}");
            }

            Type serviceInterfaceType = binding.serviceInterface;

            HashSet<Type> types = new HashSet<Type>();
            GetHierarchyPath(implementationType, serviceInterfaceType, types );
            string msg = $"for service {service.ToString()} : ";
            foreach (Type type in types)
            {
                // Debug.Log($"Adding access interface {type.Name}");
                ServiceProvider.AddService(type, service);
            }
            return service as IService;
        }

        private static bool GetHierarchyPath(Type current, Type target, HashSet<Type> result)
        {
            if(current == null)
            {
                return false;
            }

            if(current == target)
            {
                result.Add(target);
                return true;
            }

            bool foundInBaseTypes = GetHierarchyPath(current.BaseType, target, result);

            Type[] interfaces = current.GetInterfaces();
            foreach (Type i in interfaces)
            {
                if(GetHierarchyPath(i, target, result))
                {
                    foundInBaseTypes = true;
                }
            }

            if(foundInBaseTypes)
            {
                result.Add(current);
            }

            return false;
        }

        [CheatFunction(CheatCategory.Global,true)]
        public static void ReportServices()
        {
            string message = ServiceProvider.Report(new StringBuilder()).ToString();
            Debug.Log(message);
            CheatMessages.Display(message, 30);
        }


        private static void ReportServicesAtStart(string value)
        {
            if (value == nameof(reportServicesAtStart) && reportServicesAtStart)
            {
                ReportServices();
            }
        }

#if TESTS
        interface ITarget{}
        interface IDeadEnd {}
        interface IIntermediate : ITarget {}
        interface IIntermediate2 : ITarget, IDeadEnd {}

        class TestBase : IIntermediate, IIntermediate2{}
        class Test : TestBase, ITarget{}


        [CheatFunction(CheatCategory.Global, true)]
        private static void TestHierachyPath()
        {
            List<Type> list = new List<Type>();
            GetHierarchyPath(typeof(Test), typeof(ITarget), list);
            foreach (Type type in list)
            {
                Debug.Log(type.Name);
            }
        }
#endif
    }
}