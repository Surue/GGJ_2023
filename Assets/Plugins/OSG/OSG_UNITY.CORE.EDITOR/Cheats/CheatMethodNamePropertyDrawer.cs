using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(CheatMethodName))]
    public class CheatMethodNamePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 16;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative("Name");
            Rect r = position;

            string text = string.IsNullOrEmpty(name.stringValue) ? "<Select cheat>" : name.stringValue;

            CheatMethodName cheatMethodName;

            if (property.GetObjectValue<CheatMethodName>(out cheatMethodName))
            {
                EditorGUI.BeginProperty(position, label, property);
                
                if (EditorGUI.DropdownButton(r, new GUIContent(text), FocusType.Keyboard))
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
                EditorGUI.EndProperty();
            }
        }
    }
}