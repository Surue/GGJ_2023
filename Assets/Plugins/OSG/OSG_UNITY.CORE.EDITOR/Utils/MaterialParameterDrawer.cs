using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(MaterialParameter))]           
    public class MaterialParameterDrawer : PropertyDrawer
    {

        const float elementHeight = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return elementHeight * 3;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty typeProperty = property.FindPropertyRelative("parameterType");
            
            Rect r = position;
            r.height = elementHeight;
            EditorGUI.PropertyField(r, nameProperty);
            r.y += r.height;
            EditorGUI.PropertyField(r, typeProperty, new GUIContent("Parameter Type"));
            r.y += r.height;

            SerializedProperty valueProperty;
            
            string typeTxt = typeProperty.enumNames[typeProperty.enumValueIndex];
            switch(typeTxt)
            {
                case "Float": valueProperty = property.FindPropertyRelative("floatValue"); break;
                case "Color": valueProperty = property.FindPropertyRelative("colorValue"); break;
                case "Int": valueProperty = property.FindPropertyRelative("intValue"); break;
                case "Keyword": valueProperty = property.FindPropertyRelative("keywordValue"); break;
                default: valueProperty=null;break;
            }

            if(valueProperty == null)
            {
                EditorGUI.LabelField(r, "What is a " + typeTxt + " ?");
            }
            else
            {
                EditorGUI.PropertyField(r,valueProperty);
            }

        }
    }
}