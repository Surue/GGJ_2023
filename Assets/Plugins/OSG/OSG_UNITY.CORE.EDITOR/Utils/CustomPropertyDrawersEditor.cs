using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace OSG
{
    // Toggle buttons
    [CustomPropertyDrawer(typeof(ToggleButtons))]
    public class ToggleButtonsDrawer : PropertyDrawer
    {
        ToggleButtons toggleButtons { get { return (ToggleButtons)attribute; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Cache
            Color guiColor = GUI.color;
            float labelWidth = EditorGUIUtility.labelWidth;
            float buttonWidth = (position.width - labelWidth) / 2f - 2;
            Rect labelRect = position.SetWidth(labelWidth);
            Rect button1Rect = position.XOffset(labelWidth);
            button1Rect = button1Rect.SetWidth(buttonWidth);
            Rect button2Rect = button1Rect.XOffset(button1Rect.width + 4);
            bool prefabOverride = property.prefabOverride;

            // Prefab override feedback
            if (prefabOverride)
                EnhancedEditorGUI.DrawPrefabOverrideFeedback(position);

            // GUI
            string labelText = string.IsNullOrEmpty(toggleButtons.label) ? label.text : toggleButtons.label;
            GUI.Label(labelRect, new GUIContent(label) { text = prefabOverride ? labelText.InBold() : labelText }, EnhancedGUI.richText);

            GUI.color = property.boolValue ? Color.green : Color.white;
            if (GUI.Button(button1Rect, toggleButtons.option1))
            {
                property.boolValue = true;
            }

            GUI.color = property.boolValue ? Color.white : Color.red;
            if (GUI.Button(button2Rect, toggleButtons.option2))
            {
                property.boolValue = false;
            }

            GUI.color = guiColor;
        }
    }

    // Box
    [CustomPropertyDrawer(typeof(Box))]
    public class BoxDrawer : PropertyDrawer
    {
        Box box { get { return (Box)attribute; } }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (box.spacing + box.thickness) * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Cache
            Color guiColor = GUI.color;
            float spacing = box.spacing + box.thickness;
            Rect innerRect = new Rect(
                position.x + spacing,
                position.y + spacing,
                position.width - spacing * 2,
                position.height - spacing * 2);
            Texture2D boxTexture = Texture2D.whiteTexture;
            Rect indentedRect = EditorGUI.IndentedRect(position);
            Rect leftBoxRect = new Rect(indentedRect.x, indentedRect.y + box.thickness, box.thickness, indentedRect.height - box.thickness * 2);
            Rect upBoxRect = new Rect(indentedRect) { height = box.thickness, width = position.width - (indentedRect.x - position.x) };
            Rect rightBoxRect = leftBoxRect.XOffset(indentedRect.width - box.thickness);
            Rect downBoxRect = upBoxRect.YOffset(indentedRect.height - box.thickness);

            // Prefab override feedback
            if (property.prefabOverride)
                EnhancedEditorGUI.DrawPrefabOverrideFeedback(position);

            // Draw box out of 4 rectangles
            GUI.color = new Color(0, 0, 0, 0.25f);
            GUI.DrawTexture(leftBoxRect, boxTexture);
            GUI.DrawTexture(upBoxRect, boxTexture);
            GUI.DrawTexture(rightBoxRect, boxTexture);
            GUI.DrawTexture(downBoxRect, boxTexture);
            GUI.color = guiColor;

            EditorGUI.PropertyField(innerRect, property, label);
        }
    }

    // OnOff
    [CustomPropertyDrawer(typeof(OnOff))]
    public class OnOffDrawer : PropertyDrawer
    {
        OnOff onOff { get { return (OnOff)attribute; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty boolProperty = property.serializedObject.FindProperty(onOff.boolName);
            bool boolExists = !boolProperty.IsNull();

            // Cache
            Color guiColor = GUI.color;
            bool guiEnabled = GUI.enabled;
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect labelRect = position.SetWidth(labelWidth);
            Rect propertyRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth - 32 - 4, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 32, position.y, 32, position.height);
            bool prefabOverride = property.prefabOverride || (boolExists ? boolProperty.prefabOverride : false);
            bool boolValue = boolExists ? boolProperty.boolValue : false;

            // Prefab override feedback
            if (prefabOverride)
                EnhancedEditorGUI.DrawPrefabOverrideFeedback(position);

            // GUI
            GUI.enabled = (boolExists && onOff.enableWithBool) ? boolValue : true;

            string labelText = string.IsNullOrEmpty(onOff.label) ? label.text : onOff.label;
            GUI.Label(labelRect, new GUIContent(label) { text = prefabOverride ? labelText.InBold() : labelText }, EnhancedGUI.richText);

            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);

            GUI.enabled = boolExists;
            GUI.color = boolExists ? (boolValue ? Color.green : Color.red) : Color.yellow;
            if (GUI.Button(buttonRect, boolExists ? (boolValue ? "On" : "Off") : "?"))
            {
                boolProperty.boolValue = !boolValue;
            }
            GUI.enabled = guiEnabled;

            GUI.color = guiColor;
        }
    }

    // Label
    [CustomPropertyDrawer(typeof(Label))]
    public class LabelDrawer : PropertyDrawer
    {
        Label label { get { return (Label)attribute; } }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Prefab override feedback
            if (property.prefabOverride)
                EnhancedEditorGUI.DrawPrefabOverrideFeedback(position);

            EditorGUI.PropertyField(position, property, new GUIContent(this.label.label));
        }
    }
}