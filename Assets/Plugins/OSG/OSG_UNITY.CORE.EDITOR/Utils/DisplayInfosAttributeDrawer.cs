// Old Skull Games
// Pierre Planeau
// Wednesday, January 09, 2019

using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(DisplayInfosAttribute))]
    public class DisplayInfosAttributeDrawer : PropertyDrawer
    {
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DisplayInfosAttribute infosAtt = (DisplayInfosAttribute)attribute;
            string infos = GetInfosToDisplay(infosAtt, property);
            GUIContent infosGUIContent = new GUIContent(infos);

            GUI.Label(position, infosGUIContent, GetStyle());
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            DisplayInfosAttribute infosAtt = (DisplayInfosAttribute)attribute;
            string infos = GetInfosToDisplay(infosAtt, property);
            GUIContent infosGUIContent = new GUIContent(infos);

            float currentViewWidth = EditorGUIUtility.currentViewWidth - 17f; // removing 17f because the inspectors have magins
            float oneTextLineHeight = GUI.skin.label.CalcSize(new GUIContent(" ")).y;

            var style = GetStyle();
            float styleMinimumHeight = style.CalcSize(new GUIContent()).y - oneTextLineHeight;

            var textSize = style.CalcSize(infosGUIContent);
            int nbTextLines = Mathf.RoundToInt(textSize.y / oneTextLineHeight);

            return styleMinimumHeight + (oneTextLineHeight * (Mathf.Floor(textSize.x / currentViewWidth) + nbTextLines));
        }

        public GUIStyle GetStyle()
        {
            var style = GUI.skin.box;
            style.wordWrap = true;
            style.richText = true;
            style.normal.textColor = GUI.skin.label.normal.textColor;
            style.alignment = TextAnchor.MiddleCenter;

            return style;
        }

        private string GetInfosToDisplay(DisplayInfosAttribute condHAtt, SerializedProperty property)
        {
            if (property.propertyPath.Contains(".Array."))
                return "";

            DisplayInfosAttribute.InstanceProviderDelegate provider = () =>
            {
                object owner = property.GetOwner();
                //property.GetRealObjectFieldInfo(out owner);
                return owner;
            };
            return condHAtt.GetInfosToDisplay(fieldInfo.DeclaringType, provider);
        }
    }
}
