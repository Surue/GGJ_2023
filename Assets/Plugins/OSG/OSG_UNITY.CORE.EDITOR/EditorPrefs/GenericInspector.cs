using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OSG
{
    public static class GenericInspector
    {
        public static bool OnGUI(FieldInfo info, string label = null, object instance = null)
        {
            if (string.IsNullOrEmpty(label))
            {
                label = ObjectNames.NicifyVariableName(info.Name);
            }
            return Call(info, label, instance);
        }

        // bloody generics can't use enum as parameter, so do it for each needed type
        static TextAnchor TextAnchorGUI(string label, TextAnchor value, params GUILayoutOption[] options)
        {
            return (TextAnchor) EditorGUILayout.EnumPopup(value, options);
        }

        static Slider.Direction DirectionGUI(string label, Slider.Direction value, params GUILayoutOption[] options)
        {
            return (Slider.Direction) EditorGUILayout.EnumPopup(value, options);
        }

        static OSGEditorMode.Mode ModeGUI(string label, OSGEditorMode.Mode value, params GUILayoutOption[] options)
        {
            return (OSGEditorMode.Mode)EditorGUILayout.EnumPopup(value, options);
        }


        private static bool Call(FieldInfo info, string label, object instance)
        {
            switch (info.FieldType.Name)
            {
                case "Single": return OnGUIProp<float>(info, label, instance, EditorGUILayout.FloatField);
                case "Double": return OnGUIProp<double>(info, label, instance, EditorGUILayout.DoubleField);
                case "Vector2": return OnGUIProp<Vector2>(info, label, instance, EditorGUILayout.Vector2Field);
                case "Vector3": return OnGUIProp<Vector3>(info, label, instance, EditorGUILayout.Vector3Field);
                case "Vector4": return OnGUIProp<Vector4>(info, label, instance, EditorGUILayout.Vector4Field);
                case "Int32":   return OnGUIProp<int>(info, label, instance, EditorGUILayout.IntField);
                case "Color": return OnGUIProp<Color>(info, label, instance, EditorGUILayout.ColorField);
                case "Boolean": return OnGUIProp<bool>(info, label, instance, EditorGUILayout.Toggle);
                case "String": return OnGUIProp<string>(info, label, instance, EditorGUILayout.TextField);
                case "TextAnchor": return OnGUIProp<TextAnchor>(info, label, instance, TextAnchorGUI);
                case "Direction":  return OnGUIProp<Slider.Direction>(info, label, instance, DirectionGUI);
                case "Mode": return OnGUIProp<OSGEditorMode.Mode>(info, label, instance, ModeGUI);
                default:
                    EditorGUILayout.HelpBox("Add Type '" + info.FieldType.Name + "' to GenericInspector.Call()", MessageType.Warning);
                    return OnGUIFallback(info, label, instance);
            }
        }



        private static bool OnGUIQuaternion(FieldInfo info, string label, object instance)
        {
            EditorGUILayout.LabelField(label + " " + info.FieldType.Name + " " + ObjectNames.NicifyVariableName(info.Name));
            return false;
        }

        private static bool OnGUIFallback(FieldInfo info, string label, object instance)
        {
            object value = info.GetValue(instance);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label + "(" + info.FieldType.Name + ")");
            EditorGUILayout.HelpBox(value == null ? "NULL" : value.ToString(), MessageType.None);
            EditorGUILayout.EndHorizontal();

            return false;
        }

        private delegate T EditorDelegate<T>(string label, T value, params GUILayoutOption[] options);


        private static bool OnGUIProp<T>(FieldInfo info, string label, object instance, EditorDelegate<T> fieldEditor)
        {
            T value = (T) info.GetValue(instance);
            value = fieldEditor(label, value);
            if (GUI.changed)
            {
                info.SetValue(instance, value);
            }
            return GUI.changed;
        }
    }
}