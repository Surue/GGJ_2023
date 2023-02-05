// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019


using UnityEngine;
using UnityEditor;

namespace OSG
{
    public abstract class EditorCenteredCurveDrawer<T> : EditorCurveDrawer<T>
    {
        public EditorCenteredCurveDrawer()
        {
            CurrentOffset = -size * maxGraphDimensionFactor / 2f;
        }

        protected override void OnInternalGUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(title);

                GraphRect = GUILayoutUtility.GetRect(size.x, size.x, size.y, size.y);
                EditorGUI.DrawRect(GraphRect, backgroundColor);

                GraphViewRect = new Rect(GraphRect.x, GraphRect.y, maxGraphDimensionFactor * GraphRect.width, maxGraphDimensionFactor * GraphRect.height);

                GUIStyle xAxisStyle = new GUIStyle();
                xAxisStyle.normal.textColor = xAxisColor;

                GUIStyle yAxisStyle = new GUIStyle();
                yAxisStyle.normal.textColor = yAxisColor;

                GUI.BeginScrollView(GraphRect, -CurrentOffset, GraphViewRect);
                {
                    Vector2 lastOffset = CurrentOffset;
                    Vector2 lastZoom = CurrentZoom;

                    CurrentOffset = EditorGUIExtension.OffsetField(GraphViewRect, () => //Serve to nothing for the moment
                    {
                        EditorGUIExtension.DrawCurveGrid(GraphViewRect, gridLineSpacing, gridLineWidth, backgroundColor, gridColor);

                        EditorGUIExtension.DrawCenteredVerticalCurveScale(GraphViewRect, CurrentZoom.y,  (yAxisLineSpacing + gridLineSpacing / 2), yAxisLineWidth, xAxisSegmentLength, yAxisColor);

                        GUI.Label(new Rect(new Vector2(GraphViewRect.x + GraphViewRect.width * 0.5f, GraphViewRect.y + GraphViewRect.height * 0.5f) + new Vector2(-100, -50), new Vector3(40, 20)), yAxisName, xAxisStyle);

                        EditorGUIExtension.DrawCenteredHorizontalCurveScale(GraphViewRect, CurrentZoom.x,  (xAxisLineSpacing + gridLineSpacing / 2), xAxisLineWidth, yAxisSegmentLength, xAxisColor);
                        GUI.Label(new Rect(new Vector2(GraphViewRect.x + GraphViewRect.width * 0.5f, GraphViewRect.y + GraphViewRect.height * 0.5f) + new Vector2(50, 25), new Vector3(40, 20)), xAxisName, yAxisStyle);

                        CurrentZoom = EditorGUIExtension.ZoomField(GraphViewRect, () =>
                        {
                            GUI.BeginGroup(GraphViewRect);
                            {
                                EditorGUIExtension.DrawCurve(cachedDrawPoints, new Vector3(GraphViewRect.width * 0.5f, GraphViewRect.height * 0.5f, 0), graphLineWidth, curveColor, curveAreaColor);
                            }GUI.EndGroup();

                            }, CurrentZoom, true, false);
                    }
                , CurrentOffset, false, false); //Serve to nothing for the moment

                    if (!useGraphOffset)
                        CurrentOffset = lastOffset;

                    if (!useGraphZoom)
                        CurrentZoom = lastZoom;
                }
                GUI.EndScrollView();

                if (GUILayout.Button("Reset to center"))
                    CurrentOffset = -new Vector2(GraphViewRect.width - GraphRect.width, GraphViewRect.height - GraphRect.height) / 2f;
            }
            GUILayout.EndVertical();
        }
    }
}
