using System;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    class WaitForKey : EditorWindow
    {
        private Action<KeyCode> pressedAction;
        private string text;

        public static void Open(Rect r, Action<KeyCode> pressedAction, string text)
        {
            var w = CreateInstance<WaitForKey>(); //GetWindow<WaitForKeyWindow>();
            w.pressedAction = pressedAction;
            w.text = text;
            w.position = r;
            w.ShowAsDropDown(r, r.size);
        }

        public static void Open(Rect r, SerializedProperty key, string text, Action onChange=null)
        {
            Open(r, code =>
            {
                SetEnum(key, code);
                if (onChange != null) onChange();
            }, text);
        }

        private static void SetEnum(SerializedProperty key, KeyCode keycode)
        {
            try
            {
                var enums = Enum.GetNames(typeof(KeyCode));
                for (var index = 0; index < enums.Length; index++)
                {
                    if (keycode.ToString() == enums[index])
                    {
                        key.enumValueIndex = index;
                        key.serializedObject.ApplyModifiedProperties();
                        return;
                    }
                }
            }
            catch
            {
            }
        }
        GUIStyle styleForText;
        void OnGUI()
        {
            Focus();
            try
            {
                if(styleForText==null)
                {
                    styleForText = new GUIStyle(GUI.skin.label)
                    {
                        richText = true
                    };
                }
                
                if (pressedAction.Target == null)
                {
                    Close();
                    return;
                }

                GUILayout.Label(text, styleForText);
                if (Event.current == null) return;
                if (Event.current.type == EventType.KeyDown)
                {
                    KeyCode currentKeyCode = Event.current.keyCode;
                    pressedAction(currentKeyCode != KeyCode.Escape ? currentKeyCode : KeyCode.None);
                    Close();
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                Close();
            }
        }
    }
}