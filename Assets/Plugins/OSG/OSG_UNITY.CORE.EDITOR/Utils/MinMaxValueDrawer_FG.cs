using UnityEngine;
using UnityEditor;

namespace FigmentGames
{
    //TODO : eliminate one of the MinValue definition, after checking which is the best
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
                if (property.floatValue < minValue.minValue)
                    property.floatValue = minValue.minValue;
            }
            else
            {
                if (property.intValue < (int)minValue.minValue)
                    property.intValue = (int)minValue.minValue;
            }

            // Redraw actual property
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = $"{label.text} - (Min: {(propertyIsFloat ? minValue.minValue :(int)minValue.minValue)})"});
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
                if (property.floatValue > maxValue.maxValue)
                    property.floatValue = maxValue.maxValue;
            }
            else
            {
                if (property.intValue > (int)maxValue.maxValue)
                    property.intValue = (int)maxValue.maxValue;
            }

            // Redraw actual property
            EditorGUI.PropertyField(position, property, new GUIContent(label) { text = $"{label.text} - (Max: {(propertyIsFloat ? maxValue.maxValue : (int)maxValue.maxValue)})" });
        }
    }
}
