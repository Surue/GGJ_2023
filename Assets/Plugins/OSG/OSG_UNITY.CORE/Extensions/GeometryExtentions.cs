// Old Skull Games
// Pierre Planeau
// Monday, January 15, 2018

using System;
using UnityEngine;

namespace OSG
{
    public static class Geometry
    {
        public static Vector3[] GenerateCirclePoints(Vector3 center, float radius, Vector3 axis, int segments = 100)
        {
            Vector3[] points = new Vector3[segments];

            Quaternion q = Quaternion.AngleAxis(360f / segments, axis);
            Vector3 baseVector = Vector3.Cross(axis.MostNormalAxis(), axis).normalized * radius;

            for (int i = 0; i < segments; i++)
            {
                points[i] = baseVector + center;
                baseVector = q * baseVector;
            }
            
            return points;
        }

        public static void DrawCircle(Vector3 center, float radius, Vector3 axis, Action<Vector3, Vector3> DrawLine, int segments = 100)
        {
            Vector3[] points = GenerateCirclePoints(center, radius, axis, segments);

            for (int i = points.Length; --i > 0;)
            {
                DrawLine(points[i], points[i - 1]);
            }

            DrawLine(points[0], points[points.Length - 1]);
        }

        public static void DrawCircleGizmo(Vector3 center, float radius, Color color, int segments = 100)
        {
            Gizmos.color = color;
            DrawCircle(center, radius, Vector3.up, Gizmos.DrawLine, segments);
        }


        public static void DrawLineSphere(Vector3 center, float radius, Action<Vector3, Vector3> DrawLine, int lineSegments = 100)
        {
            DrawCircle(center, radius, Vector3.up, DrawLine, lineSegments);
            DrawCircle(center, radius, Vector3.forward, DrawLine, lineSegments);
            DrawCircle(center, radius, Vector3.right, DrawLine, lineSegments);
        }
    }
}