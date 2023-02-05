// Old Skull Games
// Bernard Barthelemy
// Wednesday, August 28, 2019


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    class LocalizationSelectorWindow : EditorWindow
    {
        public static void Choose(SerializedProperty property)
        {
            var window = EditorWindow.GetWindow<LocalizationSelectorWindow>(true, property.displayName, true);
            window.SelectFor(property);
        }

        private DisplayCollection<string> displayCollection;

        SerializedProperty target;
        private void SelectFor(SerializedProperty property)
        {
            target = property;
            allKeys = Localization.Instance.LocalizationKeys;
            displayCollection = new DisplayCollection<string>(allKeys, DrawKey, Matches, 19) {Filter = _f};
        }

        private void DrawKey(string key)
        {
            string val = Localization.Localize(key);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(key, GUILayout.Width(buttonSize.x)))
                {
                    target.stringValue = key;
                    target.serializedObject.ApplyModifiedProperties();
                    Close();
                }
                GUILayout.Label(val);
            }
            EditorGUILayout.EndHorizontal();
        }

        private Vector2 buttonSize;
        private static string _f;
  
        private bool Matches(string[] split, string val)
        {
            val = val.ToUpper();
            return split.All(s => val.Contains(s));
        }

        Vector2 renderSize;
        private int firstDisplay;
        private int displayCount;
        private List<string> allKeys=new List<string>();


        void OnGUI()
        {
            if(displayCollection==null)
                return;
            if (buttonSize == Vector2.zero)
            {
                foreach (var key in allKeys)
                {
                    var size = GUI.skin.button.CalcSize(new GUIContent(key));
                    if (size.x > buttonSize.x)
                    {
                        buttonSize = size;
                    }
                }
            }

            Rect controlRect = EditorGUILayout.GetControlRect(false);
            controlRect.height = position.height;
            
            displayCollection.OnGUI(controlRect);
            _f = displayCollection.Filter;
        }
    }
}
