using System;
using System.Reflection;
using OSG.Core.EventSystem;
using UnityEditor;
using UnityEngine;

namespace OSG.EventSystem.Editor
{
    [CustomPropertyDrawer(typeof(EventSource))]
    public class EventSourceDrawer : PropertyDrawer
    {
        const float height = 16;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height*2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = height;
            SerializedProperty evtName = property.FindPropertyRelative("eventName");
            evtName.GameEventField(position);
            if (string.IsNullOrEmpty(evtName.stringValue))
                return;

            position.y += height;
            FieldInfo eventInfo = EventContainerHelper.GetEventFieldInfo(evtName.stringValue);
            if (eventInfo == null)
            {
                EditorGUI.HelpBox(position, "No available " + evtName.stringValue, MessageType.Error);
            }
            else
            {
                Type eventType = eventInfo.FieldType;
                MethodInfo mI;
                var parameters = EventContainerHelper.GetEventParameters(eventType, out mI);
                if (parameters.Length == 0)
                {
                    EditorGUI.LabelField(position, "<No Parameters>");
                }
                else
                {
                    Type paramType = parameters[0].ParameterType;
                    string path = paramType.Name + "Parameter";
                    SerializedProperty pars = property.FindPropertyRelative(path);
                    pars = pars ?? property.FindPropertyRelative("ObjectParameter");


                    if (pars == null)
                        EditorGUI.LabelField(position, "NULL Parameter " + path);
                    else
                    {
                        EditorGUI.PropertyField(position, pars);
                    }
                }
            }
        }
    }
}
