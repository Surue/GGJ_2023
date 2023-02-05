using UnityEngine;
using UnityEditor;
using System;

namespace OSG
{
    public class EnhancedEditorGUI
    {
        /// <summary>
        /// Draws the little blue rectangle next to a SerializedProperty that has been modified.
        /// </summary>
        public static void DrawPrefabOverrideFeedback(Rect position)
        {
            Color guiColor = GUI.color;
            GUI.color = EnhancedGUI.prefabOverrideColor;
            Rect rect = new Rect(0, position.y - 1, 2, position.height + 2);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = guiColor;
        }

        /// <summary>
        /// Draw a GUI box and retrives the inner rect.
        /// </summary>
        public static Rect Box(Rect position, int thickness = 1, int spacing = 2)
        {
            // Cache
            Color guiColor = GUI.color;
            Texture2D boxTexture = Texture2D.whiteTexture;
            Rect indentedRect = EditorGUI.IndentedRect(position);
            Rect leftBoxRect = new Rect(indentedRect.x, indentedRect.y + thickness, thickness, indentedRect.height - thickness * 2);
            Rect upBoxRect = new Rect(indentedRect) { height = thickness, width = position.width - (indentedRect.x - position.x) };
            Rect rightBoxRect = leftBoxRect.XOffset(indentedRect.width - thickness);
            Rect downBoxRect = upBoxRect.YOffset(indentedRect.height - thickness);

            // Draw box out of 4 rectangles
            GUI.color = new Color(0, 0, 0, 0.25f);
            GUI.DrawTexture(leftBoxRect, boxTexture);
            GUI.DrawTexture(upBoxRect, boxTexture);
            GUI.DrawTexture(rightBoxRect, boxTexture);
            GUI.DrawTexture(downBoxRect, boxTexture);
            GUI.color = guiColor;

            return new Rect(
                position.x + spacing,
                position.y + spacing,
                position.width - spacing * 2,
                position.height - spacing * 2);
        }
    }
}