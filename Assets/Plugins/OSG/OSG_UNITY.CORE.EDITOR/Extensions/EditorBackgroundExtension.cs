// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019


using UnityEngine;

namespace OSG
{
    public static partial class EditorGUIExtension
    {

        public static void DrawGrid(Rect rect, Texture2D gridTexture, Texture2D crossTexture, float zoom, Vector2 panOffset)
        {
            rect.position = Vector2.zero;

            Vector2 center = rect.size / 2f;

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTexture.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTexture.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTexture.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTexture.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTexture, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTexture, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }
    }
}
