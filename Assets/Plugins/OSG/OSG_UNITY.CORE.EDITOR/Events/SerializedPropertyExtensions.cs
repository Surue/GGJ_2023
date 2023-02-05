using System;
using System.Reflection;
using OSG.Core.EventSystem;
using UnityEditor;
using UnityEngine;

namespace OSG.EventSystem.Editor
{
    public static class EventSerializedPropertyExtensions
    {
        public static void GameEventField(this SerializedProperty eventName, Rect rect)
        {
            Rect textRect = rect;
            textRect.width *= 0.25f;
            Rect gadgetRect = rect;
            gadgetRect.x += textRect.width;
            gadgetRect.width -= textRect.width;
            eventName.GameEventField(textRect, gadgetRect);
        }

        public static void GameEventField(this SerializedProperty eventName, Rect textRect, Rect gadgetRect)
        {
            string label = eventName.stringValue;
            if (string.IsNullOrEmpty(label))
                label = "<select event>";
            EditorGUI.LabelField(textRect, new GUIContent(eventName.displayName));
            if (EditorGUI.DropdownButton(gadgetRect, new GUIContent(label), FocusType.Keyboard, SerializedPropertyExtensions.DropButtonStyle))
            {
                GenericMenu menu = new GenericMenu();
                var infos = EventContainerHelper.GetAllEventInfos();
                int eventCount = infos.Count;

                for (int i = 0; i < eventCount; ++i)
                {
                    FieldInfo fieldInfo = infos[i];
                    string name = fieldInfo.Name;
                    if (name == label) continue;
                    string payloadType;
                    var parameters = EventContainerHelper.GetEventParameters(fieldInfo.FieldType, out var mI);
                    if (parameters.Length == 0)
                    {
                        payloadType = "Void";
                    }
                    else
                    {
                        Type paramType = parameters[0].ParameterType;
                        payloadType = paramType.Name;
                    }

                    menu.AddItem(new GUIContent(payloadType + "/" + name), false, () =>
                    {
                        eventName.stringValue = name;
                        eventName.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.DropDown(gadgetRect);
            }
        }
    }
}