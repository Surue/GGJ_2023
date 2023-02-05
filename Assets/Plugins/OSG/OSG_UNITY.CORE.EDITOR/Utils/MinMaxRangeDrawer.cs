/* MinMaxRangeDrawer.cs
* by Eddie Cameron – For the public domain
* ———————————————————–
* — EDITOR SCRIPT : Place in a subfolder named ‘Editor’ —
* ———————————————————–
* Renders a MinMaxRange field with a MinMaxRangeAttribute as a slider in the inspector
* Can slide either end of the slider to set ends of range
* Can slide whole slider to move whole range
* Can enter exact range values into the From: and To: inspector fields
*
*/

using UnityEngine;
using UnityEditor;

namespace OSG
{
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var range = attribute as MinMaxRangeAttribute;
            var minValue = property.FindPropertyRelative("rangeStart");
            var maxValue = property.FindPropertyRelative("rangeEnd");

            if (property.type == "MinMaxRange")
            {
                var newMin = minValue.floatValue;
                var newMax = maxValue.floatValue;

                var height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginProperty(position, label, property);

                if (!property.serializedObject.isEditingMultipleObjects)
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, height), label);

                    newMin = Mathf.Clamp(EditorGUI.FloatField(new Rect(EditorGUIUtility.labelWidth + 15, position.y, EditorGUIUtility.fieldWidth, height), newMin), range.minLimit, newMax);

                    float sliderWidth = position.width - EditorGUIUtility.labelWidth - (EditorGUIUtility.fieldWidth * 2) - 13;
                    EditorGUI.MinMaxSlider(new Rect(EditorGUIUtility.labelWidth + 70, position.y, sliderWidth, height), ref newMin, ref newMax, range.minLimit, range.maxLimit);

                    newMax = Mathf.Clamp(EditorGUI.FloatField(new Rect(position.x + position.width - 50, position.y, 50, height), newMax), newMin, range.maxLimit);

                    newMin = float.Parse(newMin.ToString("F2"));
                    newMax = float.Parse(newMax.ToString("F2"));

                    minValue.floatValue = newMin;
                    maxValue.floatValue = newMax;
                }
                else
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, height), label);
                    EditorGUI.LabelField(new Rect(EditorGUIUtility.labelWidth + 15, position.y, 500, height), "Multi-editing disabled");
                }

                EditorGUI.EndProperty();

                
            }
            else if (property.type == "MinMaxRangeInt")
            {
                float newMin = minValue.intValue;
                float newMax = maxValue.intValue;

                var xDivision = position.width * 0.395f;
                var height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginProperty(position, label, property);

                if (!property.serializedObject.isEditingMultipleObjects)
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, height), label);

                    newMin = Mathf.Clamp(EditorGUI.IntField(new Rect(position.x + xDivision, position.y, 50, height), (int)newMin), range.minLimit, newMax);
                    EditorGUI.MinMaxSlider(new Rect(position.x + xDivision + 55, position.y, position.width - xDivision - 110, height), ref newMin, ref newMax, range.minLimit, range.maxLimit);
                    newMax = Mathf.Clamp(EditorGUI.IntField(new Rect(position.x + position.width - 50, position.y, 50, height), (int)newMax), newMin, range.maxLimit);

                    newMin = float.Parse(newMin.ToString("F2"));
                    newMax = float.Parse(newMax.ToString("F2"));

                    minValue.intValue = (int)newMin;
                    maxValue.intValue = (int)newMax;
                }
                else
                {
                    EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, height), label);
                    EditorGUI.LabelField(new Rect(position.x + xDivision, position.y, 500, height), "Multi-editing disabled");
                    //EditorGUI.PropertyField(new Rect(position.x + xDivision, position.y, 50, height), minValue, new GUIContent("Min"));
                    //EditorGUI.PropertyField(new Rect(position.x + position.width * 0.5f, position.y, 50, height), maxValue, new GUIContent("Max"));
                }
                
                EditorGUI.EndProperty();
            }
        }
    }
}