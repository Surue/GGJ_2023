// Old Skull Games
// Benoit Constantin
// Monday, June 03, 2019


using UnityEngine;
using UnityEditor;

namespace OSG
{
    public abstract class EditorOnSceneCurveDrawer<T> : EditorCurveDrawer<T>
    {
        public bool drawgGrid = true;
        public Vector2 scenePosition;
        public SceneView sceneView;

        protected override void OnInternalGUI()
        {

            Handles.matrix = Matrix4x4.TRS(scenePosition,
                  Quaternion.identity,
                  Vector3.one);

            GUILayout.Label(title);

                
                GraphViewRect = new Rect(-0.5f*maxGraphDimensionFactor * size.x, 
                    - 0.5f*maxGraphDimensionFactor * size.y,
                    maxGraphDimensionFactor * size.x, 
                    maxGraphDimensionFactor * size.y);


                 GUIStyle xAxisStyle = new GUIStyle();
                xAxisStyle.normal.textColor = xAxisColor;

                GUIStyle yAxisStyle = new GUIStyle();
                yAxisStyle.normal.textColor = yAxisColor;

             if(drawgGrid)
                EditorGUIExtension.DrawCurveGrid(GraphViewRect, gridLineSpacing, gridLineWidth, backgroundColor, gridColor);

                EditorGUIExtension.DrawCenteredVerticalCurveScale(GraphViewRect, CurrentZoom.y,   gridLineSpacing, yAxisLineWidth, xAxisSegmentLength, yAxisColor);

                GUI.Label(new Rect(new Vector2(GraphViewRect.x + GraphViewRect.width * 0.5f, GraphViewRect.y + GraphViewRect.height * 0.5f) + new Vector2(-100, -50), new Vector3(40, 20)), yAxisName, xAxisStyle);

                EditorGUIExtension.DrawCenteredHorizontalCurveScale(GraphViewRect, CurrentZoom.x,  gridLineSpacing, xAxisLineWidth, yAxisSegmentLength, xAxisColor);
                GUI.Label(new Rect(new Vector2(GraphViewRect.x + GraphViewRect.width * 0.5f, GraphViewRect.y + GraphViewRect.height * 0.5f) + new Vector2(50, 25), new Vector3(40, 20)), xAxisName, yAxisStyle);


            EditorGUIExtension.DrawCurve(cachedDrawPoints, Vector2.zero, graphLineWidth, curveColor, curveAreaColor);

            Handles.matrix = Matrix4x4.identity;
        }

    }
}