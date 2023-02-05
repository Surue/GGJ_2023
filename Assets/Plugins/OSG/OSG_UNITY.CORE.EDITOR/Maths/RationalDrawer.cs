// Old Skull Games
// Pierre Planeau
// Friday, January 19, 2018

using UnityEngine;
using UnityEditor;
using OSG.Core;

namespace OSG
{
    [CustomPropertyDrawer(typeof(Rational))]
    public class RationalDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var amountRect = new Rect(position.x, position.y, 30, position.height);
            var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
            var floatValRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            SerializedProperty propertyNumerator = property.FindPropertyRelative("_numerator");
            SerializedProperty propertyDenominator = property.FindPropertyRelative("_denominator");

            EditorGUI.PropertyField(amountRect, propertyNumerator, GUIContent.none);
            EditorGUI.PropertyField(unitRect, propertyDenominator, GUIContent.none);

            if (propertyDenominator.intValue == 0)
                propertyDenominator.intValue = 1;

            Rational r = new Rational(propertyNumerator.intValue, propertyDenominator.intValue);

            EditorGUI.LabelField(floatValRect, "(" + (float)r + ")");

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
