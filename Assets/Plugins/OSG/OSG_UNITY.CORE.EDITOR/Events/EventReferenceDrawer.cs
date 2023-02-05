// Old Skull Games
// Bernard Barthelemy
// Tuesday, April 3, 2018

using OSG.EventSystem;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(EventReferenceAttribute))]
    public class EventReferenceDrawer : ConditionalHideFunctionAttributeDrawer
    {
        protected override void Render(Rect position, SerializedProperty property, GUIContent label)
        {
            string value = property.stringValue;
            if(string.IsNullOrEmpty(value))
            {
                value = "<select event>";
            }

            Rect[] rects = position.HorizontalSplit(40,60);
            GUI.Label(rects[0], "Event");

            if(EditorGUI.DropdownButton(rects[1], new GUIContent(value), FocusType.Passive ))
            {
                var menu = new GenericMenu();
                EventReferenceAttribute refatt = attribute as EventReferenceAttribute;
                foreach (var info in refatt.EventNames)
                {
                    string n = info.Name;
                    menu.AddItem(new GUIContent(n[0].ToString().ToUpper() + "/" + n), n==value,
                        () =>
                        {
                            property.stringValue = n;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                }
                menu.DropDown(rects[1]);
            }
        }
    }
}