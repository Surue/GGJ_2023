using UnityEngine;
using UnityEditor;

namespace OSG
{
    [CustomPropertyDrawer(typeof(MinValue))]
    public class MinValueDrawer : PropertyDrawer
    {
        MinValue minValue { get { return (MinValue)attribute; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool propertyIsFloat = property.propertyType == SerializedPropertyType.Float;

            // Property is not an int or a float
            if (!propertyIsFloat && property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (propertyIsFloat) // Property is a float
            {
                if (property.floatValue < minValue.minFloat)
                    property.floatValue = minValue.minFloat;
            }
            else // Property is an int
            {
                if (property.intValue < minValue.minInt)
                    property.intValue = minValue.minInt;
            }

            // Redraw actual property
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = $"{label.text} - (Min: {(propertyIsFloat ? minValue.minFloat.ToString() : minValue.minInt.ToString())})" });
        }
    }

    [CustomPropertyDrawer(typeof(MaxValue))]
    public class MaxValueDrawer : PropertyDrawer
    {
        MaxValue maxValue { get { return (MaxValue)attribute; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool propertyIsFloat = property.propertyType == SerializedPropertyType.Float;

            // Property is not an int or a float
            if (!propertyIsFloat && property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (propertyIsFloat) // Property is a float
            {
                if (property.floatValue > maxValue.maxFloat)
                    property.floatValue = maxValue.maxFloat;
            }
            else // Property is an int
            {
                if (property.intValue > maxValue.maxInt)
                    property.intValue = maxValue.maxInt;
            }

            // Redraw actual property
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = $"{label.text} - (Max: {(propertyIsFloat ? maxValue.maxFloat.ToString() : maxValue.maxInt.ToString())})" });
        }
    }
}
