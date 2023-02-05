using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public abstract class EditorCurveDrawer<T> : IEditorGUIDrawer
    {
        public string title;
        public string xAxisName;
        public string yAxisName;
        public Vector2 size = new Vector2(500, 500);
        public float maxGraphDimensionFactor = 5f;
        public int padding = 10;

        public Color backgroundColor = Color.black;
        public Color gridColor = Color.grey;
        public Color xAxisColor = Color.green;
        public Color yAxisColor = Color.green;

        public Color curveColor = Color.red;
        public Color curveAreaColor = new Color(0.2f, 0, 0, 0.5f);

        public float gridLineWidth = 2;
        public float gridLineSpacing = 40;

        public int xAxisLineWidth = 2;
        public float xAxisSegmentLength = 10;
        public int xAxisLineSpacing = 70;

        public int yAxisLineWidth = 2;
        public float yAxisSegmentLength = 10;
        public int yAxisLineSpacing = 50;

        public bool useGraphZoom = true;
        public bool useGraphOffset = true;

        public int graphLineWidth = 2;

        public string xAxisFormat = "0";
        public string yAxisFormat = "0";


        private Vector2 currentZoom = Vector2.one;
        public Vector2 CurrentZoom
        {
            get { return currentZoom; }
            set
            {
                if (currentZoom.x != value.x || currentZoom.y != value.y)
                    needSample = true;

                currentZoom = value;
            }
        }
        private Vector2 currentOffset = Vector2.zero;
        public Vector2 CurrentOffset
        {
            get { return currentOffset; }
            set
            {
                if (currentOffset.x != value.x || currentOffset.y != value.y)
                    needSample = true;

                currentOffset = value;
            }
        }

        /// <summary>
        /// A new point for draw is created if threshold is passed (it's a percent of the width/height of the graphView)
        /// </summary>
        private float drawThreshold = 0.01f;
        public float DrawThreshold
        {
            get { return drawThreshold; }
            set
            {
                if (value != drawThreshold)
                {
                    drawThreshold = value;
                    needSample = true;
                }
            }
        }

        public bool needSample { get; private set; }

        public bool forceRedraw = true;

        private T[] sampledInputs = new T[0];

        protected Vector3[] cachedDrawPoints = new Vector3[0];


        private Rect graphRect;
        /// <summary>
        /// The rect where the graph is drawn
        /// </summary>
        public Rect GraphRect
        {
            get { return graphRect; }
            protected set
            {
                if (Event.current.type == EventType.Repaint) //Otherwise it will give wrong rect
                {
                    if (graphRect.x != value.x || graphRect.y != value.y || graphRect.width != value.width || graphRect.height != value.height)
                    {
                        needSample = true;
                    }

                    graphRect = value;
                }
            }
        }


        private Rect graphViewRect;
        /// <summary>
        /// The rect where you can view the graph (not in totallity)
        /// </summary>
        public Rect GraphViewRect
        {
            get { return graphViewRect; }
            protected set
            {
                if (Event.current.type == EventType.Repaint)  //Otherwise it will give wrong rect
                {

                    if (graphViewRect.x != value.x || graphViewRect.y != value.y || graphViewRect.width != value.width || graphViewRect.height != value.height)
                    {
                        needSample = true;
                    }

                    graphViewRect = value;
                }
            }
        }

        public void OnGUI()
        {
            if (needSample)
            {
                InternalSample(drawThreshold);
                needSample = false;
            }

            OnBeginInternalGUI();
            OnInternalGUI();
            OnEndInternalGUI();
        }

        protected virtual void OnBeginInternalGUI() { }
        protected abstract void OnInternalGUI();
        protected virtual void OnEndInternalGUI() { }

        /// <summary>
        /// Internal, used by the system to sample function and call user function
        /// </summary>
        private void InternalSample(float threshold)
        {
            OnBeginSample();

            float startX = (-GraphRect.width * 0.5f - currentOffset.x) / currentZoom.x;

            GetSampledInputs(ref sampledInputs, startX, GraphViewRect.width / currentZoom.x);

            List<Vector3> points = new List<Vector3>(2 * sampledInputs.Length);

            if (sampledInputs.Length > 0)
            {
                Vector3 point = Sample(sampledInputs[0]) * currentZoom;
                point.y *= -1;
                points.Add(point);

                Vector3 lastDerivative = Vector3.zero;
                Vector3 lastPointDerivative = point;
                Vector3 lastPoint = point;

                for (int i = 1; i < sampledInputs.Length - 1; i++)
                {
                    point = Sample(sampledInputs[i]) * currentZoom;
                    point.y *= -1;

                    Vector3 pointExtrapolation = lastPointDerivative + lastDerivative * (point.x - lastPointDerivative.x);
                    if (Mathf.Abs(pointExtrapolation.x - point.x) >= (threshold * GraphViewRect.width) || Mathf.Abs(pointExtrapolation.y - point.y) >= (threshold * GraphViewRect.height))
                    {
                        points.Add(lastPoint);
                        points.Add(point);
                        lastDerivative = (point - lastPoint).normalized;
                        lastPointDerivative = point;
                    }

                    lastPoint = point;
                }

                points.Add(lastPoint);
                points.Add(Sample(sampledInputs[sampledInputs.Length - 1]) * currentZoom);
            }

            cachedDrawPoints = points.ToArray();
        }

        /// <summary>
        /// Force a resample
        /// </summary>
        public virtual void ForceReSample()
        {
            needSample = true;
        }

        /// <summary>
        /// Helper to get a proper zoom for viewing from minValue to MaxValue on x axis
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public float GetProperXZoom(float minValue, float maxValue)
        {
            float delta = maxValue - minValue;
            return GraphViewRect.width / delta;
        }

        /// <summary>
        /// Helper to get a proper zoom for viewing from minValue to MaxValue on y axis
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public float GetProperYZoom(float minValue, float maxValue)
        {
            float delta = maxValue - minValue;
            return GraphViewRect.height / delta;
        }


        /// <summary>
        /// Called before the sample is done
        /// </summary>
        public virtual void OnBeginSample() { }

        /// <summary>
        ///  Override this to Sample your function
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The point on the curve</returns>
        protected abstract Vector2 Sample(T input);


        /// <summary>
        /// Array of sampled value passed to the Sample function
        /// </summary>
        /// <param name="sampledInputs">Use the array to not garbage it if not necessary</param>
        /// <param name="startXGraphView">At which x the graph view start</param>
        /// <param name="graphViewLength">The length of the graphView (with respect of zoom)</param>
        public abstract void GetSampledInputs(ref T[] sampledInputs, float startXGraphView, float graphViewLength);


        /// <summary>
        /// Get the position of the point on the graph view
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector2 GetPointPosition(Vector2 point)
        {
            Vector2 originPosition = GraphRect.position + new Vector2(GraphRect.width, GraphRect.height) * 0.5f;
            point.y *= -1;
            point = point * currentZoom + originPosition;
            return point;
        }
    }
}
