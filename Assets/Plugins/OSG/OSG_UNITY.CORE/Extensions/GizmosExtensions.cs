// Old Skull Games
// Benoit Constantin
// Wenesday, March 20, 2019

using UnityEngine;

namespace OSG
{
    public static class GizmosExtensions
    {
        public static void DrawMarker(Vector3 position, float size, Color color, float duration, bool depthTest = true)
        {
            Gizmos.color = color;
            Vector3 line1PosA = position + Vector3.up * size * 0.5f;
            Vector3 line1PosB = position - Vector3.up * size * 0.5f;

            Vector3 line2PosA = position + Vector3.right * size * 0.5f;
            Vector3 line2PosB = position - Vector3.right * size * 0.5f;

            Vector3 line3PosA = position + Vector3.forward * size * 0.5f;
            Vector3 line3PosB = position - Vector3.forward * size * 0.5f;

            Gizmos.DrawLine(line1PosA, line1PosB);
            Gizmos.DrawLine(line2PosA, line2PosB);
            Gizmos.DrawLine(line3PosA, line3PosB);
        }

        public static void DrawVector(Vector3 position, Vector3 direction, float markerSize, Color color, float duration, bool depthTest = true)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(position, position + direction);
            DrawMarker(position + direction, markerSize, color, 0, false);
        }

        public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}
