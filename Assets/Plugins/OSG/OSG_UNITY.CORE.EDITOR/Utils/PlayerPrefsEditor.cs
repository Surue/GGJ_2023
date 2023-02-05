using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR_WIN_DEAD
using Microsoft.Win32;
#endif

namespace OSG
{
    /// <summary>
    /// Allows to show all PlayerPrefs values set in the project in an editor window.
    /// </summary>
    public class PlayerPrefsEditor : EditorWindow
    {
        [MenuItem("Edit/Player Prefs")]
        public static void openWindow()
        {

            PlayerPrefsEditor window = (PlayerPrefsEditor) EditorWindow.GetWindow(typeof(PlayerPrefsEditor));
            window.titleContent = new GUIContent("Player Prefs");
            window.Show();

        }

        public enum FieldType
        {
            String,
            Integer,
            Float
        }

        private FieldType fieldType = FieldType.String;
        private string setKey = "";
        private string setVal = "";
        private string error = null;
#if UNITY_EDITOR_WIN_DEAD
        public string keyBase;

        private Vector2 scrollPosition;

        private void AllKeysLayout()
        {
            if (string.IsNullOrEmpty(keyBase))
            {
                keyBase = "SOFTWARE\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName;
            }


            keyBase = EditorGUILayout.TextField("BASE KEY", keyBase);


            if (string.IsNullOrEmpty(keyBase)) return;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(keyBase);

            if (key == null)
            {
                EditorGUILayout.HelpBox("No keys", MessageType.Error);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            string[] valueNames = key.GetValueNames();
            List<string> subKeyNames = new List<string>(valueNames);
            subKeyNames.Sort();

            foreach (string keyName in subKeyNames)
            {
                EditKey(keyName.Substring(0, keyName.LastIndexOf('_')));
            }
            EditorGUILayout.EndScrollView();

        }

        private void EditKey(string keyName)
        {

            if (GUILayout.Button(keyName))
            {
                setKey = keyName;
            }
            GUILayout.BeginHorizontal();

            if (EditFloatKey(keyName)
                && EditIntKey(keyName)
                && EditStringKey(keyName))
            {
                GUILayout.Label("Unable to get !");
            }

            GUILayout.EndHorizontal();
        }

        private bool EditStringKey(string keyName)
        {
            const string defaultValue = null;
            string value = PlayerPrefs.GetString(keyName, defaultValue);
            if (value == defaultValue)
            {
                return true;
            }


            if (value.Length > 2048)
            {
                EditorGUILayout.HelpBox("Very long string, discard display", MessageType.Error);
            }
            else
            {
                string newValue = EditorGUILayout.TextField("string", value);
                if (newValue != value)
                {
                    PlayerPrefs.SetString(keyName, newValue);
                    PlayerPrefs.Save();
                }
            }

            try
            {
                var obj = PlayerPrefsSerializer.Load(keyName);
                if (obj != null)
                {
                    //var editor = Editor.CreateEditor(obj as UnityEngine.Object);
                    //editor.DrawDefaultInspector();
                    EditorGUILayout.HelpBox(obj.ToString(), MessageType.Info);
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private bool EditIntKey(string keyName)
        {
            const int defaultValue = int.MaxValue;
            int value = PlayerPrefs.GetInt(keyName, defaultValue);
            if (value == defaultValue)
            {
                return true;
            }

            int newValue = EditorGUILayout.IntField("int", value);
            if (newValue != value)
            {
                PlayerPrefs.SetInt(keyName, newValue);
                PlayerPrefs.Save();
            }

            return false;
        }

        private bool EditFloatKey(string keyName)
        {
            const float defaultValue = float.NaN;
            float value = PlayerPrefs.GetFloat(keyName, defaultValue);
            if (float.IsNaN(value))
            {
                return true;
            }

            float newValue = EditorGUILayout.FloatField("float", value);
            if (GUI.changed)
            {
                PlayerPrefs.SetFloat(keyName, newValue);
                PlayerPrefs.Save();
            }

            return false;
        }



#endif
        void OnGUI()
        {

            EditorGUILayout.LabelField("Player Prefs Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("by RomejanicDev");
            EditorGUILayout.Separator();

            fieldType = (FieldType) EditorGUILayout.EnumPopup("Key Type", fieldType);
            setKey = EditorGUILayout.TextField("Key to Set", setKey);
            setVal = EditorGUILayout.TextField("Value to Set", setVal);

            if (error != null)
            {

                EditorGUILayout.HelpBox(error, MessageType.Error);

            }

            if (GUILayout.Button("Set Key"))
            {

                if (fieldType == FieldType.Integer)
                {

                    int result;
                    if (!int.TryParse(setVal, out result))
                    {

                        error = "Invalid input \"" + setVal + "\"";
                        return;

                    }

                    PlayerPrefs.SetInt(setKey, result);

                }
                else if (fieldType == FieldType.Float)
                {

                    float result;
                    if (!float.TryParse(setVal, out result))
                    {

                        error = "Invalid input \"" + setVal + "\"";
                        return;

                    }

                    PlayerPrefs.SetFloat(setKey, result);

                }
                else
                {

                    PlayerPrefs.SetString(setKey, setVal);

                }

                PlayerPrefs.Save();
                error = null;

            }

            if (GUILayout.Button("Get Key"))
            {

                if (fieldType == FieldType.Integer)
                {

                    setVal = PlayerPrefs.GetInt(setKey).ToString();

                }
                else if (fieldType == FieldType.Float)
                {

                    setVal = PlayerPrefs.GetFloat(setKey).ToString();

                }
                else
                {

                    setVal = PlayerPrefs.GetString(setKey);

                }

            }

            if (GUILayout.Button("Delete Key"))
            {

                PlayerPrefs.DeleteKey(setKey);
                PlayerPrefs.Save();

            }

            if (GUILayout.Button("Delete All Keys"))
            {

                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();

            }

            EditorGUILayout.Separator();

#if UNITY_EDITOR_WIN_DEAD
            AllKeysLayout();
#else
        GUILayout.Label("NO REGISTRY KEY");
#endif

        }
    }
}