#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace OSG
{
    using static EnhancedColors;

    public partial class BezierCurve : OSGMono
    {

        [Tooltip(
            "This is the number of segments in-between two curve anchors. The higher this value is, the more accurate is the curve." +
            "\n\nA vlue of 1 results in a a linear curve.")]
        [SerializeField] private int _segmentation = 20;
        public int segmentation { get { return _segmentation; } private set { if (value < 1) value = 1; _segmentation = value; } }
        [Tooltip("The maximum number of points the curve can have.")]
        [SerializeField] private int _maxCurveLength = 100;
        public int maxCurveLength
        {
            get
            {
                return _maxCurveLength;
            }

            private set
            {
                int minValue = closeCurve ? curveAnchors.Count + 1 : curveAnchors.Count;

                if (value < minValue)
                    value = minValue;

                _maxCurveLength = value;
            }
        }
        [Tooltip(
            "Should the curve points be automatically updated when the transform has changed?" +
            "\n\nUntick this parameter for better performance.")]
        public bool transformUpdate = true;

        [Space(10)]
        [Header("GIZMOS & HANDLES")]
        [SerializeField] private bool drawCurveUnselected = true;
        [Space]
        public Color curveColor = Color.cyan;
        public Color handlesColor = lightBlue;
        public Color selectedHandleColor = orange;
        [Space]
        [SerializeField] [OnOff("drawGhost", "Ghost Curve")] private Color ghostColor = softMagenta.Alpha(0.25f);
        [SerializeField, HideInInspector] private bool drawGhost = true;
        [Space]
        [Tooltip("This determines the snap spacing when moving handles in the scene view.")]
        public Vector3 snap = Vector3.one;
        [Space]
        [Tooltip("This value determines on which axis the handles can be moved on.")]
        public Axis axis;
        [HideInInspector] public bool axisLock;
        [Tooltip("What parameter should drive the 3rd (locked) axis offset?")]
        public AxisOffset axisOffset;
        public float axisOffsetValue;

        public enum Axis
        {
            All,
            XY,
            YZ,
            ZX
        }

        public enum AxisOffset
        {
            Transform,
            TransformRelative,
            Value
        }

        // Cache
        [HideInInspector] public int lastHandleID = -1;
        public static bool showTooltips = true;
        [HideInInspector] public bool initialized;


        private BezierCurve()
        {
            ResetCurve(true);

            initialized = false;
        }

        #region BEHAVIOUR

        public void AddAnchor(Vector3 position, Vector3 leftTangent, Vector3 rightTangent, int index = -1)
        {
            if (index < 0)
                return;

            Undo.RecordObject(this, "Anchor added on BezierCurve.");

            curveAnchors.Insert(index + 1, new BezierAnchor(position, leftTangent, rightTangent));

            ClampSegmentation();
            CacheAllCurvePoints();

            EditorUtility.SetDirty(this);
        }

        public void AddAnchorAfterSelection()
        {
            // No anchor is currently selected
            if (lastHandleID == -1)
                return;

            // Cache
            bool linear = segmentation == 1;
            float defaultTangentMagnitude = BezierAnchor.defaultInTangent.magnitude;

            bool isLastAnchor = lastHandleID == curveAnchors.Count - 1; // Last anchor from the list;

            BezierAnchor bezierAnchor = curveAnchors[lastHandleID];
            BezierAnchor nextBezierAnchor = isLastAnchor ? curveAnchors[0] : curveAnchors[lastHandleID + 1];

            bool loop = isLastAnchor && !closeCurve; // Last anchor and the curve is opened
            Vector3 anchor = loop ?
                (linear ?
                bezierAnchor.anchor + (bezierAnchor.anchor - curveAnchors[lastHandleID - 1].anchor) * 0.5f :
                bezierAnchor.anchor + bezierAnchor.outTangent * 2) :
                (linear ?
                (bezierAnchor.anchor + nextBezierAnchor.anchor) * 0.5f :
                transformCached.InverseTransformPoint(CalculateCubicBezierPoint(lastHandleID, 0.5f)));

            Vector3 leftTangent = (loop ? -bezierAnchor.outTangent.normalized : (transformCached.InverseTransformPoint(CalculateCubicBezierPoint(lastHandleID, 0.49f)) - anchor).normalized) * defaultTangentMagnitude;
            Vector3 rightTangent = loop ? -leftTangent : (transformCached.InverseTransformPoint(CalculateCubicBezierPoint(lastHandleID, 0.51f)) - anchor).normalized * defaultTangentMagnitude;

            AddAnchor(anchor, leftTangent, rightTangent, lastHandleID);
        }

        public void DeleteAnchor(int index)
        {
            if (curveAnchors.Count < 3)
                return;

            Undo.RecordObject(this, "Anchor deleted on BezierCurve.");

            curveAnchors.RemoveAt(index);

            // The last anchor has been deleted
            if (index == curveAnchors.Count)
                lastHandleID = 0;

            CacheAllCurvePoints();

            EditorUtility.SetDirty(this);
        }

        public void DeleteSelectedAnchor()
        {
            DeleteAnchor(lastHandleID);
        }

        public void ResetCurve(bool fromConstructor = false)
        {
            if (!fromConstructor)
                Undo.RecordObject(this, "BezierCurve complete reset.");

            curveAnchors = new List<BezierAnchor>();
            curveAnchors.Add(new BezierAnchor(new Vector3(-5, 0, 0)));
            curveAnchors.Add(new BezierAnchor(new Vector3(5, 0, 0)));

            lastHandleID = -1;

            segmentation = 20;
            maxCurveLength = 100;
            closeCurve = false;

            previousAnchorsCount = curveAnchors.Count;
            previousSegmentation = segmentation;
            previousCloseCurve = closeCurve;

            if (!axisLock)
                axis = Axis.All;

            previousAxis = axis;
            previousAxisOffset = axisOffset;
            previousAxisOffsetValue = axisOffsetValue;

            FlattenCurve();

            if (fromConstructor)
                return;

            CacheAllCurvePoints();

            EditorUtility.SetDirty(this);
        }

        public void CacheAllCurvePoints()
        {
            int segmentsCount = closeCurve ? curveAnchors.Count : curveAnchors.Count - 1;
            curvePoints = new Vector3[segmentsCount * segmentation + 1];

            for (int i = 0; i < segmentsCount; i++)
                CacheSegmentPoints(i);

            EditorUtility.SetDirty(this);
        }

        public void CacheSegmentPoints(int curveSegment)
        {
            curvePoints[curveSegment * segmentation] = curveAnchors[curveSegment].anchor;

            for (int i = 1; i < segmentation; i++)
                curvePoints[curveSegment * segmentation + i] = CalculateLocalCubicBezierPoint(curveSegment, (float)i / segmentation);

            // Last segment
            if (curveSegment == curveAnchors.Count - (closeCurve ? 1 : 2))
                curvePoints[curvePoints.Length - 1] = curveAnchors[closeCurve ? 0 : curveAnchors.Count - 1].anchor;

            EditorUtility.SetDirty(this);
        }

        public void CloseCurve(bool close)
        {
            Undo.RecordObject(this, $"BezierCurve {(close ? "closed" : "opened")}.");

            closeCurve = close;

            ClampSegmentation();
            CacheAllCurvePoints();

            EditorUtility.SetDirty(this);
        }

        private void ClampSegmentation()
        {
            int maxSegmentation = (maxCurveLength - 1) / (closeCurve ? curveAnchors.Count : curveAnchors.Count - 1);
            segmentation = Mathf.Clamp(segmentation, 1, maxSegmentation);
        }

        public void ResetSelectedAnchorTangent()
        {
            ResetAnchorTangent(lastHandleID);
        }

        public void ResetAnchorTangent(int anchorID)
        {
            // Out of range
            if (anchorID < 0 || anchorID > curveAnchors.Count - 1)
                return;

            Undo.RecordObject(this, "Anchor reset on BezierCurve.");

            curveAnchors[anchorID] = new BezierAnchor(curveAnchors[anchorID].anchor, BezierAnchor.defaultInTangent, -BezierAnchor.defaultInTangent);

            FlattenCurve();
            if (anchorID > 0)
                CacheSegmentPoints(anchorID - 1);
            if (anchorID < curveAnchors.Count - 1)
                CacheSegmentPoints(anchorID);

            EditorUtility.SetDirty(this);
        }

        #endregion

        #region UNITY

        [SerializeField] private int previousAnchorsCount;
        [SerializeField] private int previousSegmentation;
        [SerializeField] private bool previousCloseCurve;

        private void OnValidate()
        {
            if (curveAnchors.Count < 2)
                ResetCurve();

            // Hard set
            segmentation = segmentation;
            maxCurveLength = maxCurveLength;

            // Inspector check
            if (previousAnchorsCount != curveAnchors.Count)
            {
                CacheAllCurvePoints();
                previousAnchorsCount = curveAnchors.Count;
            }

            if (previousSegmentation != segmentation)
            {
                CacheAllCurvePoints();
                previousSegmentation = segmentation;
            }

            if (previousCloseCurve != closeCurve)
            {
                CloseCurve(closeCurve);
                previousCloseCurve = closeCurve;
            }
        }

        [SerializeField] private Vector3 previousPosition;
        [SerializeField] private Quaternion previousRotation;
        [SerializeField] private Vector3 previousScale;
        [SerializeField] private float hasMovedTime;
        [SerializeField] private float previousTime;
        [SerializeField] private bool hasBeenUpdated;

        [SerializeField] private Axis previousAxis;
        [SerializeField] private AxisOffset previousAxisOffset;
        [SerializeField] private float previousAxisOffsetValue;

        private void OnDrawGizmos()
        {
            // Cannot be done in constructor, so there it is...
            if (!initialized)
            {
                CacheAllCurvePoints();
                initialized = true;
            }

            // Transform update
            bool hasMoved =
                previousPosition != transformCached.position ||
                previousRotation != transformCached.rotation ||
                previousScale != transformCached.lossyScale;

            if (transformUpdate)
            {
                if (hasMoved)
                {
                    if (axis != Axis.All)
                        FlattenCurve();

                    CacheAllCurvePoints();
                }
            }
            else
            {
                hasMovedTime = hasMoved ? 0f : hasMovedTime + (float)EditorApplication.timeSinceStartup - previousTime;

                if (hasMoved)
                    hasBeenUpdated = false;

                // Wait the end of the transform edit
                if (!hasBeenUpdated && hasMovedTime > 0.2f)
                {
                    if (axis != Axis.All)
                        FlattenCurve();

                    CacheAllCurvePoints();
                    hasBeenUpdated = true;
                }
            }

            previousPosition = transformCached.position;
            previousRotation = transformCached.rotation;
            previousScale = transformCached.lossyScale;
            previousTime = (float)EditorApplication.timeSinceStartup;

            // Axis edit
            if (previousAxis != axis ||
                previousAxisOffset != axisOffset ||
                previousAxisOffsetValue != axisOffsetValue)
            {
                FlattenCurve();
                CacheAllCurvePoints();
            }
            previousAxis = axis;
            previousAxisOffset = axisOffset;
            previousAxisOffsetValue = axisOffsetValue;

            if (drawCurveUnselected)
                DrawCurve();
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawCurveUnselected)
                DrawCurve();
        }

        #endregion

        #region GIZMOS

        private void DrawCurve()
        {
            Handles.color = curveColor;

            // Segmented curve draw
            int length = curvePoints.Length;
            Vector3[] worldCurvePoints = new Vector3[length];
            for (int i = 0; i < length; i++)
                worldCurvePoints[i] = transformCached.TransformPoint(curvePoints[i]);
            Handles.DrawAAPolyLine(worldCurvePoints);

            if (segmentation < 2)
                return;

            // Actual curve draw
            int count = closeCurve ? curveAnchors.Count : curveAnchors.Count - 1;
            for (int i = 0; i < count; i++)
            {
                BezierAnchor a1 = curveAnchors[i];
                BezierAnchor a2 = curveAnchors[i == curveAnchors.Count - 1 ? 0 : i + 1];

                Handles.DrawBezier(
                    transformCached.TransformPoint(a1.anchor),
                    transformCached.TransformPoint(a2.anchor),
                    transformCached.TransformPoint(a1.anchor + a1.outTangent),
                    transformCached.TransformPoint(a2.anchor + a2.inTangent),
                    ghostColor, null, 2);
            }
        }

        public void FlattenPoint(ref Vector3 point, Vector3 sourceOffset = default)
        {
            switch (axis)
            {
                case Axis.XY:
                    point.z = sourceOffset.z;
                    break;

                case Axis.YZ:
                    point.x = sourceOffset.x;
                    break;

                case Axis.ZX:
                    point.y = sourceOffset.y;
                    break;
            }
        }

        public void FlattenAnchor(BezierAnchor bezierAnchor)
        {
            if (axis == Axis.All)
                return;

            Vector3 vectorOffset = GetAxisOffsetPoint();

            switch (axis)
            {
                case Axis.XY:
                    bezierAnchor.anchor = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor).ZValue(vectorOffset.z));
                    bezierAnchor.inTangent = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.inTangent).ZValue(vectorOffset.z)) - bezierAnchor.anchor;
                    bezierAnchor.outTangent = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.outTangent).ZValue(vectorOffset.z)) - bezierAnchor.anchor;
                    break;

                case Axis.YZ:
                    bezierAnchor.anchor = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor).XValue(vectorOffset.x));
                    bezierAnchor.inTangent = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.inTangent).XValue(vectorOffset.x)) - bezierAnchor.anchor;
                    bezierAnchor.outTangent = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.outTangent).XValue(vectorOffset.x)) - bezierAnchor.anchor;
                    break;

                case Axis.ZX:
                    bezierAnchor.anchor = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor).YValue(vectorOffset.y));
                    bezierAnchor.inTangent = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.inTangent).YValue(vectorOffset.y)) - bezierAnchor.anchor;
                    bezierAnchor.outTangent = transformCached.InverseTransformPoint(transformCached.TransformPoint(bezierAnchor.anchor + bezierAnchor.outTangent).YValue(vectorOffset.y)) - bezierAnchor.anchor;
                    break;
            }
        }

        public Vector3 GetAxisOffsetPoint()
        {
            switch (axisOffset)
            {
                case AxisOffset.Transform:
                    return transformCached.position;

                case AxisOffset.TransformRelative:
                    return transformCached.position + Vector3.one * axisOffsetValue;

                case AxisOffset.Value:
                    return Vector3.one * axisOffsetValue;
            }

            return Vector3.zero;
        }

        public void FlattenCurve()
        {
            for (int i = 0; i < curveAnchors.Count; i++)
                FlattenAnchor(curveAnchors[i]);
        }

        #endregion
    }
}
#endif