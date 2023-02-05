// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019

using UnityEngine;
using UnityEditor;
using System;

namespace OSG
{
    public abstract class EditorBorderedCurveDrawer<T> : EditorCurveDrawer<T>
    {
        private Rect areaRect;
        public Rect AreaRect
        {
            get { return areaRect; }
            private set
            {
                if (Event.current.type == EventType.Repaint)  //Otherwise it will give wrong rect
                    areaRect = value;
            }
        }

        /// <summary>
        /// First argument : CurrentZoom, Second argument : OriginPosition
        /// </summary>
        public Action<Vector2, Vector2> OnDrawCurve;

        public EditorBorderedCurveDrawer()
        {
            CurrentOffset = -size * maxGraphDimensionFactor / 2f;
        }

        protected override void OnInternalGUI()
        {
            GUILayout.BeginVertical();
            {
                GUIStyle titleStyle = new GUIStyle();
                titleStyle.normal.textColor = curveColor;
                titleStyle.fontSize = 15;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.padding = new RectOffset(10,10,0,10);

                GUILayout.Label(title, titleStyle);

                AreaRect = GUILayoutUtility.GetRect(size.x, size.x, size.y, size.y);
                EditorGUI.DrawRect(AreaRect, backgroundColor);

                GraphViewRect = new Rect(AreaRect.x + 80, AreaRect.y, AreaRect.width - 80, AreaRect.height - 30);
                GraphRect = new Rect(GraphViewRect.x, GraphViewRect.y, maxGraphDimensionFactor * GraphViewRect.width, maxGraphDimensionFactor * GraphViewRect.height);

                GUIStyle xAxisStyle = new GUIStyle();
                xAxisStyle.normal.textColor = xAxisColor;

                GUIStyle yAxisStyle = new GUIStyle();
                yAxisStyle.normal.textColor = yAxisColor;


                GUI.BeginScrollView(GraphViewRect, -CurrentOffset, GraphRect, GUIStyle.none, GUIStyle.none);
                {
                    Vector2 lastOffset = CurrentOffset;
                    Vector2 lastZoom = CurrentZoom;

                    CurrentOffset = EditorGUIExtension.OffsetField(GraphRect, () =>
                    {
                        EditorGUIExtension.DrawCurveGrid(GraphRect, gridLineSpacing, gridLineWidth, backgroundColor, gridColor);

                        CurrentZoom = EditorGUIExtension.ZoomField(GraphRect, () =>
                        {
                            GUI.BeginGroup(GraphRect);
                            {
                                EditorGUIExtension.DrawCurve(cachedDrawPoints, new Vector3(GraphRect.width * 0.5f, GraphRect.height * 0.5f, 0), graphLineWidth, curveColor, curveAreaColor);

                            }
                            GUI.EndGroup();

                            OnDrawCurve?.Invoke(CurrentZoom, GraphRect.position + new Vector2(GraphRect.width, GraphRect.height) * 0.5f);

                        }, CurrentZoom, false, false);
                    }
                , CurrentOffset, false, false);


                    if (!useGraphOffset)
                        CurrentOffset = lastOffset;

                    if (!useGraphZoom)
                        CurrentZoom = lastZoom;
                }
                GUI.EndScrollView();

                EditorGUIExtension.DrawVerticalCurveScale(new Vector2(GraphViewRect.position.x, GraphViewRect.position.y + GraphViewRect.height * 0.5f), GraphViewRect.height, 0.5f * GraphRect.height - 0.5f * GraphViewRect.height + CurrentOffset.y, CurrentZoom.y, gridLineSpacing * ((yAxisLineSpacing + gridLineSpacing / 2) / gridLineSpacing), yAxisLineWidth, xAxisSegmentLength, yAxisColor, yAxisFormat);
                EditorGUIExtension.DrawHorizontalCurveScale(new Vector2(GraphViewRect.position.x + GraphViewRect.width * 0.5f, GraphViewRect.position.y + GraphViewRect.height), GraphViewRect.width, 0.5f * GraphRect.width - 0.5f * GraphViewRect.width + CurrentOffset.x, CurrentZoom.x, gridLineSpacing * ((xAxisLineSpacing + gridLineSpacing / 2) / gridLineSpacing), xAxisLineWidth, yAxisSegmentLength, xAxisColor, xAxisFormat);

                GUI.Label(new Rect(new Vector2(GraphViewRect.x, GraphViewRect.y) + new Vector2(10, 20), new Vector3(40, 20)), yAxisName, yAxisStyle);
                GUI.Label(new Rect(new Vector2(GraphViewRect.x + GraphViewRect.width, GraphViewRect.y + GraphViewRect.height) + new Vector2(-45, -40), new Vector3(40, 20)), xAxisName, yAxisStyle);

                if (useGraphOffset && GUILayout.Button("Reset to center"))
                    CurrentOffset = -new Vector2(GraphRect.width - GraphViewRect.width, GraphRect.height - GraphViewRect.height) / 2f;
            }
            GUILayout.EndVertical();
        }
    }
}
