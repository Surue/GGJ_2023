// Old Skull Games
// Bernard Barthelemy
// Friday, November 8, 2019

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using OSG.Core;
using UnityEditor.SceneManagement;

namespace OSG.Services
{
    [CustomPropertyDrawer(typeof(ServiceBinding))]
    class ServiceBindingDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.GetObjectValue(out ServiceBinding serviceBinding);

            if(serviceBinding==null)
            {
                serviceBinding = new ServiceBinding(){
                    serviceType =  new SerializedService()
                    };
                property.SetObjectValue(serviceBinding);
            }

            Type serviceType = serviceBinding.serviceType;
            if(serviceType == typeof(void))
            {
                ServiceSelectionDropDown(property, serviceBinding, position, "Select service implementation class");
            }
            else
            {
                position.width /= 2;
                ServiceSelectionDropDown(property, serviceBinding, position, serviceType.Name);
                position.x += position.width;
                InterfaceSelectionDropDown(property, serviceBinding, position);
            }
        }


        private static readonly Type[] forbiddenTypes = {
            null,
            typeof(void),
            typeof(object),
            typeof(IService)
        };

        private static void GetTypes(Type currentType, HashSet<Type> types)
        {
            if(forbiddenTypes.Contains(currentType))
                return;

            types.Add(currentType);
            var interfaces = currentType.GetInterfaces();
            foreach (Type @interface in interfaces)
            {
                GetTypes(@interface, types);
            }
            GetTypes(currentType.BaseType, types);
        }


        private void InterfaceSelectionDropDown(SerializedProperty property, ServiceBinding serviceBinding, Rect position)
        {
            if(serviceBinding.serviceInterface == null)
            {
                serviceBinding.serviceInterface = new SerializedServiceInterface();
                serviceBinding.serviceInterface.SetEffectiveType(serviceBinding.serviceType);
            }

            Type interfaceType = serviceBinding.serviceInterface;
            string label = interfaceType == null ? "Select Interface type" : interfaceType.FullName;
            if(EditorGUI.DropdownButton(position, new GUIContent(label), FocusType.Keyboard))
            {
                var types = new HashSet<Type>();
                GetTypes(serviceBinding.serviceType, types);
                GenericMenu menu = new GenericMenu();
                foreach (Type type in types)
                {
                    menu.AddItem(new GUIContent(type.Name), serviceBinding.serviceInterface == type, () => { 
                        serviceBinding.serviceInterface.SetEffectiveType(type);
                        UpdateTarget(property);
                    });
                    menu.ShowAsContext();
                }
            }
        }

        private void ServiceSelectionDropDown(SerializedProperty property, ServiceBinding serviceBinding, Rect position, string label)
        {
            if(EditorGUI.DropdownButton(position, new GUIContent(label), FocusType.Keyboard))
            {
                Type currentType = serviceBinding.serviceType ?? typeof(void);
                GenericMenu menu = new GenericMenu();
                AssemblyScanner.Register(type =>
                {
                    if (type.IsAbstract)
                    {
                        return;
                    }

                    if(!type.DerivesFrom(typeof(IService)))
                    {
                        return;
                    }
                    string menuLabel = type.Name;
                    menu.AddItem(new GUIContent(menuLabel), type == currentType,
                        () =>
                        {
                            serviceBinding.serviceType.SetEffectiveType(type);
                            UpdateTarget(property);
                        }
                    );
                }, AssemblyScanner.OnlyProject);
                AssemblyScanner.Scan(() => menu.ShowAsContext());
            }
        }

        private static void UpdateTarget(SerializedProperty property)
        {
            UnityEngine.Object target = property.serializedObject.targetObject;
            EditorUtility.SetDirty(target);
            Component c = target as Component;
            if (c && c.gameObject.scene.rootCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
            }
        }
    }
}
