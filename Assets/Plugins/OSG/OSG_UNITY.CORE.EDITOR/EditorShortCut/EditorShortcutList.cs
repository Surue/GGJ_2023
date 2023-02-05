using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public class EditorShortcutList : ScriptableObject
    {
        private EditorSavedBool IconsHidden;
        private EditorSavedBool IgnoreKeys;
        private EditorSavedBool ShowOnControl;

        void OnEnable()
        {
            IconsHidden =   new EditorSavedBool("InconsHidden_" + name);
            IgnoreKeys =    new EditorSavedBool("IgnoreKeys_" + name);
            ShowOnControl = new EditorSavedBool("ShowOnControl_" + name);
        }

        private static List<EditorShortcutList> instances;
        public static List<EditorShortcutList> Instances
        {
            get
            {
                if (instances == null)
                {
                    string[] findAssets = AssetDatabase.FindAssets("t:EditorShortcutList");
                    instances = new List<EditorShortcutList>();

                    for (var index= 0; index < findAssets.Length; index++)
                    {
                        string asset = findAssets[index];
                        string path = AssetDatabase.GUIDToAssetPath(asset);
                        var i = AssetDatabase.LoadAssetAtPath<EditorShortcutList>(path);
                        if(i)
                        {
                            instances.Add(i);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Can't Load " + path);
                        }
                    }

                    if (instances != null)
                    {
                        foreach (EditorShortcutList instance in instances)
                        {
                            instance.Rebind();
                        }
                    }
                }
                return instances;
            }
        }

        private void Rebind()
        {
            foreach (EditorShortcutKeys key in keys)
            {
                if (key.type == ShortcutType.CallStaticMethod)
                {
                    var i = key.method.Info;
                }
            }
        }

        public EditorShortcutKeys[] keys;
        public void UpdateGUI(ref Rect r, Vector2 direction, GUIStyle style)
        {
            if (keys == null) return;
            Event current = Event.current;
            ManageIcons(ref r, current, direction, style);
            ManageKeys(current);
        }

        private void ManageKeys(Event current)
        {
            if (IgnoreKeys) return;
            if (current == null) return;

            //if (!current.control) return;
            //if (!current.shift) return;
            if (current.type != EventType.KeyUp) return;

            for (var index = 0; index < keys.Length; index++)
            {
                EditorShortcutKeys key = keys[index];
                if (key.code != current.keyCode) continue;
                key.Execute();
            }
        }

        private void ManageIcons(ref Rect r, Event current, Vector2 direction, GUIStyle style)
        {
            if (IconsHidden && ! (current.control && current.alt))
            {
                if(!(ShowOnControl && current.control))
                {
                    return;    
                }
            }
            bool edit = current.control && current.alt;
            Color color = GUI.color;
            Color backgroundColor = GUI.backgroundColor;
            if (edit)
            {
                GUI.color = Color.red;
                GUI.backgroundColor = Color.red;
            }

            for (var index = 0; index < keys.Length; index++)
            {
                EditorShortcutKeys key = keys[index];
                if(key == null)
                    continue;
                if (!key.icon) continue;
                if (GUI.Button(r, new GUIContent(key.icon, key.ToString()), style))
                {
                    if (edit)
                    {
                        Selection.activeObject = this;
                        this.FocusInspector();
                        CustomEditorBase.autofocus = name+"."+"keys"+index;
                    }
                    else
                        key.Execute();
                }
                r.y += r.height * direction.y;
                r.x += r.width * direction.x;
            }

            GUI.color = color;
            GUI.backgroundColor = backgroundColor;
        }

        public void OnInspectorGUI()
        {
            bool h = EditorGUILayout.Toggle("Hide Icons", IconsHidden);
            if(h != IconsHidden)
            {
                IconsHidden.Value = h;
            }
            h = EditorGUILayout.Toggle("Ignore Keys", IgnoreKeys);
            if(h != IgnoreKeys)
            {
                IgnoreKeys.Value = h;
            }
            h = EditorGUILayout.Toggle("Shows on Control Key", ShowOnControl);
            if(h != ShowOnControl)
            {
                ShowOnControl.Value = h;
            }
        }
    }


}