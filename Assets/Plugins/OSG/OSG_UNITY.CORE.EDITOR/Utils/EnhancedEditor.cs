using UnityEngine;
using UnityEditor;
using System;

namespace OSG
{
    using static EnhancedGUI;

    public class EnhancedEditor
    {
        // Prefs
        public static string editorKeyPrefix { get { return $"{PlayerSettings.companyName}.{PlayerSettings.productName}.".CleanName('.'); } }

        // Colors
        public static Color lightGray { get { return new Color(0.75f, 0.75f, 0.75f, 1); } }
        public static Color warningColor { get { return new Color(1, 0.75f, 0.25f, 1); } }

        #region PROPERTY

        public static SerializedProperty PropertyField(SerializedObject serializedObject, string propertyName, bool enableEdit)
        {
            return PropertyField(serializedObject, propertyName, default, enableEdit);
        }

        public static SerializedProperty PropertyField(SerializedObject serializedObject, string propertyName, string label = default, bool enableEdit = true)
        {
            if (!CheckProperty(serializedObject, propertyName, out SerializedProperty serializedProperty))
                return null;

            return PropertyField(serializedProperty, label, enableEdit);
        }

        public static SerializedProperty PropertyField(SerializedProperty serializedProperty, bool enableEdit)
        {
            return PropertyField(serializedProperty, default, enableEdit);
        }

        public static SerializedProperty PropertyField(SerializedProperty serializedProperty, string label = default, bool enableEdit = true)
        {
            // Cache
            bool guiEnabled = GUI.enabled;
            Color guiColor = GUI.color;

            GUI.enabled = enableEdit;

            if (serializedProperty.isArray || // Array
                serializedProperty.propertyType == SerializedPropertyType.Generic) // Custom class
            {
                GUI.color = guiColor.Alpha(0.25f);
                GUILayout.BeginHorizontal("box");
                GUI.color = guiColor;

                GUILayout.Space(12);
            }

            EditorGUILayout.PropertyField(serializedProperty, new GUIContent(string.IsNullOrEmpty(label) ? serializedProperty.name.NiceName() : label), true);

            if (serializedProperty.isArray)
                GUILayout.EndHorizontal();

            GUI.enabled = guiEnabled;

            return serializedProperty;
        }


        public static void GUIPropertyField(Rect rect, SerializedProperty serializedProperty, bool enableEdit)
        {
            GUIPropertyField(rect, serializedProperty, default, enableEdit);
        }

        public static void GUIPropertyField(Rect rect, SerializedProperty serializedProperty, string label = default, bool enableEdit = true)
        {
            bool guiEnabled = GUI.enabled;
            GUI.enabled = enableEdit;

            EditorGUI.PropertyField(rect, serializedProperty, new GUIContent(string.IsNullOrEmpty(label) ? serializedProperty.name.NiceName() : label)); // Use an empty text to keep the draggable-edit behaviour

            GUI.enabled = guiEnabled;
        }

        #endregion

        #region HELPERS

        public static void SmallSpace()
        {
            GUILayout.Space(6);
        }

        public static void LargeSpace()
        {
            GUILayout.Space(10);
        }


        public static bool CheckProperty(SerializedObject serializedObject, string propertyName, out SerializedProperty serializedProperty)
        {
            serializedProperty = serializedObject.FindProperty(propertyName);

            // No property found
            if (serializedProperty.IsNull())
            {
                WarningBox($"No property named \"{propertyName}\" could be found.");
                return false;
            }

            return true;
        }

        public static bool CheckProperty(SerializedObject serializedObject, string propertyName, out SerializedProperty serializedProperty, SerializedPropertyType type)
        {
            serializedProperty = serializedObject.FindProperty(propertyName);

            // No property found
            if (serializedProperty.IsNull())
            {
                WarningBox($"No property named \"{propertyName}\" could be found.");
                return false;
            }

            if (!CheckPropertyType(serializedProperty, type))
                return false;

            return true;
        }

        public static bool CheckPropertyType(SerializedProperty serializedProperty, SerializedPropertyType type)
        {
            if (serializedProperty.propertyType != type)
            {
                WarningBox($"Property \"{serializedProperty.name}\" is not of type {type}.");
                return false;
            }

            return true;
        }

        public static bool StartCategory(Editor editorScript, string title, Action action)
        {
            string completeBoolName = $"{editorScript.GetType().Name}.{action.Method.Name}";
            bool display = GetEditorBool(completeBoolName);

            GUI.enabled = true;

            GUI.color = Color.white;
            GUILayout.BeginVertical("box");
            {
                GUI.color = new Color(0.8f, 0.8f, 0.8f, 1);
                GUILayout.BeginHorizontal("box");
                {
                    GUI.color = Color.white;
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(title.InBold(), categoryTitleStyle);
                        EditorResources editorResources = EditorResources.Instance;
                        Texture2D texture = Texture2D.whiteTexture;
                            
                        if(editorResources)
                            texture= display ? editorResources.eyeIcon.Colored(lightGray) : editorResources.barredEyeIcon.Colored(Color.gray);
                        if (GUILayout.Button(texture,GUILayout.Width(32), GUILayout.Height(24)))
                        {
                            SetEditorBool(completeBoolName, !display);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                if (display)
                    action.Invoke();
            }
            GUILayout.EndVertical();

            return display;
        }


        public static void WarningBox(string message)
        {
            Color startColor = GUI.color;
            GUI.color = new Color(1, 0.5f, 0.5f, 1);

            GUILayout.BeginHorizontal("box");
            {
                EditorGUILayout.LabelField(message, centeredWrapTextStyle);
            }
            GUILayout.EndHorizontal();

            GUI.color = startColor;
        }


        public static bool CenteredButton(string label, Action action, int width = 232)
        {
            bool buttonPressed = false;

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(label, GUILayout.Width(width)))
                {
                    action.Invoke();

                    buttonPressed = true;
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            return buttonPressed;
        }

        #endregion

        #region PREFS

        public static bool GetEditorBool(string name)
        {
            return EditorPrefs.GetBool($"{editorKeyPrefix}{name}");
        }

        public static void SetEditorBool(string name, bool value)
        {
            EditorPrefs.SetBool($"{editorKeyPrefix}{name}", value);
        }

        #endregion
    }
}