using System;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public class ScriptableCreatorWindow : EditorWindow
    {
        public static event Action<ScriptableObject> onCreate;
        public static event Action<ScriptableCreatorWindow> onClose;

        [MenuItem("Assets/Create/--ANY SCRIPTABLE--", false, -500)]
        public static ScriptableCreatorWindow OpenWindow()
        {
            var window = CreateInstance<ScriptableCreatorWindow>();
            window.Show();
            return window;
        }

        public static ScriptableCreatorWindow OpenWindow(string filter)
        {
            var window = OpenWindow();
            window.filter = filter;
            return window;
        }


        private string filter = "";
        private string upperFilter;
        private Vector2 scrollPos;

        
        public void OnGUI()
        {
            GUI.SetNextControlName("FilterControl");
            filter = EditorGUILayout.TextField("Filter", filter);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            bool doFilter = !string.IsNullOrEmpty(filter);
            
            if (doFilter)
            {
                upperFilter = filter.ToUpper();
            }
            bool any = false;
            Type lastType = null;


            int typeCount=0;
            foreach (Type scriptableType in ScriptableObjectUtility.ScriptableTypes)
            {
                if (doFilter && !scriptableType.FullName.ToUpper().Contains(upperFilter)) continue;
                any = true;
                lastType = scriptableType;
                ++typeCount;
                if (GUILayout.Button(new GUIContent(scriptableType.FullName)))
                {
                    Create(scriptableType);
                }
            }
            if (!any)
            {
                EditorGUILayout.HelpBox(
                    "No scriptable type containing " + filter + "\nBe aware the filter is case_sensitive!",
                    MessageType.Warning);
            }
            EditorGUILayout.EndScrollView();
            EditorGUI.FocusTextInControl("FilterControl");

            var current = Event.current;
            if (current !=null && current.isKey )
            {
                var key = current.keyCode;
                if( key == KeyCode.Escape)
                    Close();
                else if (key == KeyCode.KeypadEnter || key == KeyCode.Return && typeCount==1)
                {
                    Create(lastType);
                }
            }
        }

        private void Create(Type scriptableType)
        {
            ScriptableObject scriptableObject = ScriptableObjectUtility.CreateAsset(scriptableType);
            onCreate?.Invoke(scriptableObject);
            Close();
        }

        private void OnDestroy()
        {
            onClose?.Invoke(this);
        }
    }
}