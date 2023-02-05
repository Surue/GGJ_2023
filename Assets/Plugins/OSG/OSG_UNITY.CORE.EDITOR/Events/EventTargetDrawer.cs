using System;
using System.Collections.Generic;
using System.Reflection;
using OSG.Core.EventSystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSG.EventSystem.Editor
{
    [CustomPropertyDrawer(typeof(EventTarget))]
    public class EventTargetDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 3*TextDimension.y + 4;
        }

        private Vector2? d;

        private Vector2 TextDimension
        {
            get
            {
                if (!d.HasValue)
                {
                    d = GUI.skin.label.CalcSize(new GUIContent("Event Name")) + new Vector2(16, 2);
                }
                return d.Value;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            Rect gadgetRect = position;
            gadgetRect.height = 1;
            EditorGUI.DrawRect(gadgetRect, Color.black);

            float rY = TextDimension.y;

            gadgetRect.height = rY;
            gadgetRect.x += 8;
            gadgetRect.width -= 8;
            gadgetRect.y += 4;


            Rect textRect = gadgetRect;
            textRect.width = TextDimension.x;
            gadgetRect.x += textRect.width;
            gadgetRect.width -= textRect.width;

            position.x -= 4;
            position.width += 4;

            EditorGUI.BeginProperty(position, label, property);

            // Get the properties
            SerializedProperty eventName = property.FindPropertyRelative("eventName");
            SerializedProperty targetProperty = property.FindPropertyRelative("target");


            bool shouldClearMethod = false;


            gadgetRect.x -= 15;
            Object oldTargetComponent = targetProperty.objectReferenceValue;
            EditorGUI.LabelField(textRect, "Target");
            // don't use ObjectField's label or texts and gadgets won't align
            EditorGUI.ObjectField(gadgetRect, targetProperty, GUIContent.none);
            gadgetRect.x += 15;
            Object targetComponent = targetProperty.objectReferenceValue;
            shouldClearMethod = targetComponent != oldTargetComponent;

            if (shouldClearMethod)
            {
                eventName.stringValue = String.Empty;
                targetProperty.serializedObject.ApplyModifiedProperties();
            }

            if (!targetComponent)
            {
                return;
            }

            gadgetRect.y += rY + 2;
            textRect.y += rY + 2;

            eventName.GameEventField(textRect, gadgetRect);
            bool hasEvent = !string.IsNullOrEmpty(eventName.stringValue);
            if (hasEvent)
            {
                gadgetRect.y += rY;
                textRect.y += rY;
                EditorGUI.LabelField(textRect, "Function");

                List<MethodInfo> availableMethods = GetAvailableMethods(targetComponent, eventName.stringValue);
                if (availableMethods == null || availableMethods.Count == 0)
                {
                    EditorGUI.HelpBox(gadgetRect,
                        targetProperty.name + " has no compatible methods with " + eventName.stringValue,
                        MessageType.Error);
                }
                else
                {
                    object owner;
                    SerializedProperty methodProperty = property.FindPropertyRelative("method");
                    #pragma warning disable 618
                    FieldInfo methodFieldInfo = methodProperty.GetFieldInfoAndOwner(out owner);
                    #pragma warning restore 618

                    if (methodFieldInfo != null)
                    {
                        if (shouldClearMethod)
                        {
                            UnityEngine.Debug.Log("Clear the method we got a new one");
                            SetMethodProperty(methodProperty, methodFieldInfo, owner,
                                new SerializableMethodInfo());
                        }

                        SerializableMethodInfo mi = owner != null
                            ? methodFieldInfo.GetValue(owner) as SerializableMethodInfo
                            : null;

                        GUIContent dropButtonContent = mi == null
                            ? new GUIContent("Select one:")
                            : mi.Info.GUIContent(false);

                        if (EditorGUI.DropdownButton(gadgetRect, dropButtonContent, FocusType.Keyboard,
                            SerializedPropertyExtensions.DropButtonStyle))
                        {
                            GenericMenu menu = new GenericMenu();

                            foreach (MethodInfo info in availableMethods)
                            {
                                var info1 = info;
                                menu.AddItem(info.GUIContent(true), false, () =>
                                {
                                    mi = new SerializableMethodInfo
                                    {
                                        Info = info1,
                                        bindingFlags = (int) EventContainerHelper.MethodBindingFlags
                                    };

                                    SetMethodProperty(methodProperty, methodFieldInfo, owner, mi);
                                });
                            }
                            menu.DropDown(gadgetRect);
                        }
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        private static void SetMethodProperty(SerializedProperty methodProperty,
            FieldInfo fieldInfo,
            object targetObject,
            SerializableMethodInfo serializableMethodInfo)
        {
            Undo.RecordObject(methodProperty.serializedObject.targetObject, "Set Method");
            fieldInfo.SetValue(targetObject, serializableMethodInfo);
            methodProperty.serializedObject.ApplyModifiedProperties();
        }

        private List<MethodInfo> GetAvailableMethods(object target, string eventName)
        {
            List<MethodInfo> infos = null;
            MethodInfo eventInvokeMethodInfo;
            ParameterInfo[] eventInvokeParameters = EventContainerHelper.GetEventParameters(eventName, out eventInvokeMethodInfo);
            Type targetType = target.GetType();
            MethodInfo[] methods = targetType.GetMethods(EventContainerHelper.MethodBindingFlags);
            for (int i = 0; i < methods.Length; ++i)
            {
                MethodInfo targetMethodInfo = methods[i];
                if (!EventContainerHelper.IsCompatible(targetMethodInfo, eventInvokeParameters)) continue;
                infos = infos ?? new List<MethodInfo>();
                infos.Add(targetMethodInfo);
            }

            return infos;
        }

    }
}