using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(CheatShortcut))]
    public class CheatShortcutPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty key = property.FindPropertyRelative("key");
            SerializedProperty name = property.FindPropertyRelative("name");
            Rect r = position;
            r.width *= 0.5f;
            //EditorGUI.PropertyField(r, key);
            string text = string.IsNullOrEmpty(name.stringValue) ? "<Select cheat>" : name.stringValue;

            CheatShortcut shortcut;
            //property.GetRealObjectFieldInfo(out shortcut);

            if (property.GetObjectValue<CheatShortcut>(out shortcut))
            {
                EditorGUI.BeginProperty(position, label, property);

                string tooltip;

                Color color = GUI.color;

                if (shortcut.cheatInfo == null)
                {
                    foreach (CheatInfo cheatInfo in CheatSettings.CheatInfos)
                    {
                        if (cheatInfo.method.Name == shortcut.name)
                        {
                            shortcut.cheatInfo = cheatInfo;
                            break;
                        }
                    }

                    GUI.color = Color.red;
                    tooltip = "Unkown function";
                }
                else
                {
                    tooltip = shortcut.cheatInfo.method.DeclaringType.ToString() + "."
                            + shortcut.cheatInfo.method.Name;
                }

                if (EditorGUI.DropdownButton(r, new GUIContent(text, tooltip), FocusType.Keyboard))
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (CheatInfo info in CheatSettings.CheatInfos)
                    {
                        var info1 = info;
                        menu.AddItem(new GUIContent(info.category + "/" + info.method.Name), false, () =>
                        {
                            name.stringValue = info1.method.Name;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                    menu.DropDown(r);
                }
                r.x += r.width;
                if (GUI.Button(r, shortcut.Label()))
                {
                    r.position = GUIUtility.GUIToScreenPoint(r.position);
                    WaitForKey.Open(r, key, "Press a key for " + name.stringValue);
                }

                GUI.color = color;
                EditorGUI.EndProperty();
            }
        }
    }
}