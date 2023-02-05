// Old Skull Games
// Bernard Barthelemy
// Tuesday, October 10, 2017

using UnityEditor;
using UnityEngine;

namespace OSG
{
    [CustomPropertyDrawer(typeof(ConditionalHideFunctionAttribute))]
    public class ConditionalHideFunctionAttributeDrawer : PropertyDrawer
    {
        protected enum Draw
        {
            None,
            Disabled,
            Normal
        }


        protected Draw ShouldDraw(SerializedProperty property)
        {
            ConditionalHideFunctionAttribute condHAtt = (ConditionalHideFunctionAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);
            if(enabled)
                return Draw.Normal;
            
            return condHAtt.HideInInspector ? Draw.None : Draw.Disabled;
        }

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var shouldDraw = ShouldDraw(property);
            if (shouldDraw == Draw.None) return;

            bool wasEnabled = GUI.enabled;
            GUI.enabled = shouldDraw == Draw.Normal;
            Render(position, property, label);
            GUI.enabled = wasEnabled;
        }

        protected virtual void Render(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalHideFunctionAttribute condHAtt = (ConditionalHideFunctionAttribute)attribute;

            bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

            if (!condHAtt.HideInInspector || enabled)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        private bool GetConditionalHideAttributeResult(ConditionalHideFunctionAttribute condHAtt, SerializedProperty property)
        {
            if(property.propertyPath.Contains(".Array."))
                return true;

            ConditionalHideFunctionAttribute.InstanceProviderDelegate provider = () =>
            {
                object owner = property.GetOwner();
                //property.GetRealObjectFieldInfo(out owner);
                return owner;
            };
            return condHAtt.GetConditionalHideAttributeResult(fieldInfo.DeclaringType, provider);
        }
    }
}