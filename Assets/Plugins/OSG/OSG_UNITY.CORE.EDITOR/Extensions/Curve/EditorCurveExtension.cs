// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public static partial class EditorGUIExtension
    {
        private static MethodInfo search;
        private static bool alreadySearched;
        private static string SearchFallBack(Rect r, string s, bool _)
        {
            return EditorGUI.TextArea(r, s);
        }

        public static string SearchField(string s)
        {
            Rect rect = EditorGUILayout.GetControlRect(false);
            return SearchField(rect, s);
        }

        public static string SearchField(Rect rect, string s)
        {
            if (search != null) 
                return search.Invoke(null, new object[] {rect, s, false}) as string;

            if (!alreadySearched)
            {
                alreadySearched = true;
                var ms = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                foreach (MethodInfo info in ms)
                {
                    if (info.Name == "ToolbarSearchField")
                    {
                        if (info.GetParameters().Length == 3)
                        {
                            search = info;
                            return SearchField(rect, s);
                        }
                    }
                }
                Debug.LogWarning("Could not get ToolbarSearchField method, using fallback");
            }
            return SearchFallBack(rect, s, false);
        }


        /// <summary>
        /// Draw a vertical curve scale at the middle of the rect
        /// </summary>
        /// <param name="label"></param>
        /// <param name="rect"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="lineSpacing"></param>
        /// <param name="lineWidth"></param>
        /// <param name="segmentLength"></param>
        /// <param name="color"></param>
        public static void DrawCenteredVerticalCurveScale(Rect rect, float zoom, float lineSpacing, float lineWidth, float segmentLength, Color color, string format = "0")
        {
            DrawVerticalCurveScale(rect.position + rect.size * 0.5f, rect.height, 0, zoom, lineSpacing, lineWidth, segmentLength, color, format);
        }


        /// <summary>
        /// Draw a vertical curve scale
        /// </summary>
        /// <param name="label"></param>
        /// <param name="origin"></param>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <param name="zoom"></param>
        /// <param name="lineSpacing"></param>
        /// <param name="lineWidth"></param>
        /// <param name="segmentLength"></param>
        /// <param name="color"></param>
        public static void DrawVerticalCurveScale(Vector2 origin, float size, float offset, float zoom, float lineSpacing, float lineWidth, float segmentLength, Color color, string format = "0")
        {
            Handles.color = color;

            int numberOfPoint = (int)Math.Ceiling((size / lineSpacing)) + 1;
            Vector2 perpendicular = Vector2.left * segmentLength;


            float offsetPos = +(10000 * offset) % (10000 * lineSpacing) / 10000f;
            float offsetValue = (-(int)(offset / lineSpacing));

            //Axis line
            Handles.DrawAAPolyLine(lineWidth, new Vector2(origin.x, origin.y - size * 0.5f), new Vector2(origin.x, origin.y + size * 0.5f));

            //Special case for i == 0 ==> Not drawing 2 time the same segment
            Vector2 pos = origin;
            pos.y = origin.y + offsetPos;
            Handles.DrawAAPolyLine(lineWidth, pos, pos + perpendicular);

            for (int i = -(int)(numberOfPoint / 2f); i <= (int)(numberOfPoint / 2f); i++)
            {
                float value = ((i - offsetValue) * lineSpacing) / zoom;

                pos = origin;
                pos.y = origin.y - i * lineSpacing + offsetPos;

                if (pos.y > origin.y - size * 0.5f && pos.y < origin.y + size * 0.5f) //otherwise ==> out of screen
                {
                    Handles.DrawAAPolyLine(lineWidth, pos, pos + perpendicular);
                    GUI.Label(new Rect(pos + new Vector2(-45, -10), new Vector3(50, 20)), value.ToString(format));
                }
            }
        }


        /// <summary>
        /// Draw a horizontal curve scale at the middle of the rect
        /// </summary>
        /// <param name="label"></param>
        /// <param name="rect"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="lineSpacing"></param>
        /// <param name="lineWidth"></param>
        /// <param name="segmentLength"></param>
        /// <param name="color"></param>
        public static void DrawCenteredHorizontalCurveScale(Rect rect, float zoom, float lineSpacing, float lineWidth, float segmentLength, Color color, string format = "0")
        {
            DrawHorizontalCurveScale(rect.position + rect.size * 0.5f, rect.width, 0, zoom, lineSpacing, lineWidth, segmentLength, color, format);
        }

        /// <summary>
        /// Draw a horizontal curve scale
        /// </summary>
        /// <param name="label"></param>
        /// <param name="rect"></param>
        /// <param name="zoom"></param>
        /// <param name="lineSpacing"></param>
        /// <param name="lineWidth"></param>
        /// <param name="segmentLength"></param>
        /// <param name="color"></param>
        public static void DrawHorizontalCurveScale(Vector2 origin, float size, float offset, float zoom, float lineSpacing, float lineWidth, float segmentLength, Color color, string format = "0")
        {
            Handles.color = color;

            int numberOfPoint = (int)Math.Ceiling((size / lineSpacing)) + 1;
            Vector2 perpendicular = Vector2.up * segmentLength;

            //Axis line
            Handles.DrawAAPolyLine(lineWidth, new Vector2(origin.x - size * 0.5f, origin.y), new Vector2(origin.x + size * 0.5f, origin.y));


            float offsetPos = +(10000 * offset) % (10000 * lineSpacing) / 10000f;
            float offsetValue = (-(int)(offset / lineSpacing));

            //Special case for i == 0 ==> Not drawing 2 time the same segment
            Vector2 pos = origin;
            pos.y = origin.y;

            for (int i = -(int)(numberOfPoint / 2f); i <= (int)(numberOfPoint / 2f); i++)
            {
                float value = -((i - offsetValue) * lineSpacing) / zoom;

                pos = origin;
                pos.x = origin.x - i * lineSpacing + offsetPos;

                if (pos.x > origin.x - size * 0.5f && pos.x < origin.x + size * 0.5f) //otherwise ==> out of screen
                {
                    Handles.DrawAAPolyLine(lineWidth, pos, pos + perpendicular);
                    GUI.Label(new Rect(pos + new Vector2(0, 5), new Vector3(100, 20)), value.ToString(format));
                }
            }
        }

        /// <summary>
        /// Draw the background for curve
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="linespacing"></param>
        /// <param name="lineWidth"></param>
        /// <param name="backgroundColor"></param>
        /// <param name="gridColor"></param>
        public static void DrawCurveGrid(Rect rect, float linespacing, float lineWidth, Color backgroundColor, Color gridColor)
        {
            EditorGUI.DrawRect(rect, backgroundColor);
            Handles.color = gridColor;

            int numberOfLineX = (int)Math.Ceiling((rect.height / linespacing) / 2f);
            int numberOfLineY = (int)Math.Ceiling((rect.width / linespacing) / 2f);

            Handles.DrawAAPolyLine(lineWidth, new Vector2(rect.position.x, rect.position.y), new Vector2(rect.position.x + rect.width, rect.position.y));
            Handles.DrawAAPolyLine(lineWidth, new Vector2(rect.position.x, rect.position.y), new Vector2(rect.position.x, rect.position.y + rect.height));
            Handles.DrawAAPolyLine(lineWidth, new Vector2(rect.position.x, rect.position.y + rect.height), new Vector2(rect.position.x + rect.width, rect.position.y + rect.height));
            Handles.DrawAAPolyLine(lineWidth, new Vector2(rect.position.x + rect.width, rect.position.y), new Vector2(rect.position.x + rect.width, rect.position.y + rect.height));

            float yOrigin = rect.position.y + rect.height / 2f;

            for (int i = 0; i < numberOfLineX; i++)
            {
                float endY = i * linespacing;
                Handles.DrawAAPolyLine(lineWidth, new Vector2(rect.x, yOrigin + endY), new Vector2(rect.x + rect.width, yOrigin + endY));
                Handles.DrawAAPolyLine(lineWidth, new Vector2(rect.x, yOrigin - endY), new Vector2(rect.x + rect.width, yOrigin - endY));
            }


            float xOrigin = rect.position.x + rect.width / 2f;

            for (int i = 0; i < numberOfLineY; i++)
            {
                float endX = i * linespacing;
                Handles.DrawAAPolyLine(lineWidth, new Vector2(xOrigin + endX, rect.y), new Vector2(xOrigin + endX, rect.y + rect.height));
                Handles.DrawAAPolyLine(lineWidth, new Vector2(xOrigin - endX, rect.y), new Vector2(xOrigin - endX, rect.y + rect.height));
            }

        }


        /// <summary>
        /// Draw a curve with double linked point
        /// </summary>
        /// <param name="points"></param>
        /// <param name="rect"></param>
        /// <param name="lineWidth"></param>
        /// <param name="curveColor"></param>
        public static void DrawCurve(Vector3[] points, Vector3 centerPosition, float lineWidth, Color curveColor, Color areaColor)
        {

                Matrix4x4 lastHandleMatrix = Handles.matrix;
    
                Handles.matrix *= Handles.matrix.inverse * Matrix4x4.Translate(centerPosition) * Handles.matrix;

                Handles.color = areaColor;
                for (int i = 0; i < points.Length - 1; i++) //Totally not optimized
                {
                    Handles.DrawAAConvexPolygon(points[i], points[i+1], new Vector2(points[i+1].x, 0), new Vector2(points[i].x,0));
                }

                Handles.color = curveColor;
                Handles.DrawAAPolyLine(lineWidth, points);

                Handles.matrix = lastHandleMatrix;

        }
    }
}
