using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSG
{
    public partial class BezierCurve : OSGMono
    {
        [Space(10)]
        [Header("CURVE")]
        [Tooltip("The list of all the curve anchors that can be manipulated in the scene view.")]
        [SerializeField] private List<BezierAnchor> _curveAnchors = new List<BezierAnchor>();
        public List<BezierAnchor> curveAnchors { get { return _curveAnchors; } private set { _curveAnchors = value; } }

        // All the curve points are store in LOCAL SPACE, any calculation to get a point on the path must take this into account.
        // Storing the curve points in local space simplify the curve editing.
        [SerializeField, HideInInspector] private Vector3[] _curvePoints;
        public Vector3[] curvePoints { get { return _curvePoints; } private set { _curvePoints = value; } }

        [Tooltip("Should the curve be closed?")]
        [SerializeField] private bool _closeCurve;
        public bool closeCurve { get { return _closeCurve; } private set { _closeCurve = value; } }

        /// <summary>
        /// Retrieves a world space point in-between two anchors at position t (lerp).
        /// </summary>
        public Vector3 CalculateCubicBezierPoint(int curveSegment, float t)
        {
            return transformCached.TransformPoint(CalculateLocalCubicBezierPoint(curveSegment, t));
        }

        /// <summary>
        /// Retrieves a local space point in-between two anchors at position t (lerp).
        /// </summary>
        public Vector3 CalculateLocalCubicBezierPoint(int curveSegment, float t)
        {
            // Curve limits
            if (curveSegment < 0)
                return curvePoints[0];
            else if (curveSegment > curveAnchors.Count - (closeCurve ? 1 : 2))
                return curvePoints[curvePoints.Length - 1];

            BezierAnchor a1 = curveAnchors[curveSegment];
            BezierAnchor a2 = curveAnchors[(closeCurve && curveSegment > curveAnchors.Count - 2) ? 0 : curveSegment + 1];
            Vector3 p0 = a1.anchor;
            Vector3 p1 = a1.anchor + a1.outTangent;
            Vector3 p2 = a2.anchor + a2.inTangent;
            Vector3 p3 = a2.anchor;

            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }


        /// <summary>
        /// Gets an approximation of the closest point on the curve from a world space one. The higher the segmentation is, the more accurate is the result.
        /// </summary>
        public Vector3 GetNearestPointOnSegmentPath(Vector3 point)
        {
            // Cache
            int count = curvePoints.Length - 1;
            float minMagnitude = float.MaxValue;
            Vector3 snapPoint = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                Vector3 p = EnhancedMath.GetNearestPointOnFiniteLine(
                    transformCached.TransformPoint(curvePoints[i]),
                    transformCached.TransformPoint(curvePoints[i + 1]),
                    point);
                Vector3 snapVector = p - point;

                if (snapVector.magnitude < minMagnitude)
                {
                    snapPoint = p;
                    minMagnitude = snapVector.magnitude;
                }
            }

            return snapPoint;
        }

        // BENOIT : C'est là que ça se passe
        /// <summary>
        /// Gets an the closest point on the curve from a world space one.
        /// </summary>
        public Vector3 GetNearestPointOnCurve(Vector3 point)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Retrieves an approximation of all the intersections between the given plane and the curve.
        /// </summary>
        public Vector3[] GetPlaneIntersectionsWithSegmentPath(UnityEngine.Plane plane, int maxCount = 0)
        {
            // Cache
            int count = curvePoints.Length - 1;
            Vector3 snapPoint = Vector3.zero;
            List<Vector3> intersections = new List<Vector3>();
            int currentCount = 0;

            for (int i = 0; i < count; i++)
            {
                Vector3 p1 = transformCached.TransformPoint(curvePoints[i]);
                Vector3 p2 = transformCached.TransformPoint(curvePoints[i + 1]);
                Vector3 direction = p2 - p1;
                bool hit = plane.Raycast(new Ray(p1, direction), out float distance);

                if (!hit || distance > direction.magnitude)
                    continue;

                intersections.Add(p1 + direction.normalized * distance);

                currentCount++;
                if (maxCount > 0 && currentCount == maxCount)
                    break;
            }

            return intersections.ToArray();
        }

        // BENOIT : Et ici aussi
        /// <summary>
        /// Retrieves all the intersections between the given plane and the curve.
        /// </summary>
        public Vector3[] GetPlaneIntersectionsWithCurve(Plane plane, int maxCount = 0)
        {
            throw new NotImplementedException();
        }
    }

    [System.Serializable]
    public class BezierAnchor
    {
        public Vector3 anchor;
        public Vector3 inTangent;
        public Vector3 outTangent;

        public static Vector3 defaultInTangent = new Vector3(0, -2, 0);

        public BezierAnchor()
        {
            this.anchor = Vector3.zero;
            this.inTangent = defaultInTangent;
            this.outTangent = -defaultInTangent;
        }

        public BezierAnchor(Vector3 anchor)
        {
            this.anchor = anchor;
            this.inTangent = defaultInTangent;
            this.outTangent = -defaultInTangent;
        }

        public BezierAnchor(Vector3 anchor, Vector3 inTangent, Vector3 outTangent)
        {
            this.anchor = anchor;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }
    }
}