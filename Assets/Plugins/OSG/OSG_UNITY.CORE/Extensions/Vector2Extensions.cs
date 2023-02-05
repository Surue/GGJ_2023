// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using OSG.DebugTools;
using UnityEngine;

namespace OSG
{
    public static class Vector2Extensions
    {   
        public static bool Intersection(ref Vector2 result, Vector2 p0, Vector2 k0, Vector2 p1, Vector2 k1)
        {
            Vector2? found = FindInterSection(p0, k0, p1, k1);
            if (found.HasValue)
            {
                result.x = found.Value.x;
                result.y = found.Value.y;
                return true;
            }
            return false;
        }

        public static void ToXZ(this Vector2 v2, out Vector3 v3, float y=0)
        {
            v3.x = v2.x;
            v3.y = y;
            v3.z = v2.y;
        }

        public static void ToXY(this Vector2 v2, out Vector3 v3, float z=0)
        {
            v3.x = v2.x;
            v3.y = v2.y;
            v3.z = z;
        }

        public static void ToYZ(this Vector2 v2, out Vector3 v3, float x = 0)
        {
            v3.x = x;
            v3.y = v2.x;
            v3.z = v2.y;
        }

        public static Vector3 ToXY(this Vector2 v2, float z=0)
        {
            Vector3 v3;
            v2.ToXY(out v3, z);
            return v3;
        }

        public static Vector3 ToYZ(this Vector2 v2, float x = 0)
        {
            Vector3 v3;
            v2.ToYZ(out v3, x);
            return v3;
        }

        public static Vector3 ToXZ(this Vector2 v2, float y = 0)
        {
            Vector3 v3;
            v2.ToXZ(out v3, y);
            return v3;
        }

        // find the intersection between LINES Δ0 = p0 + k0 * t and Δ1 = p1 + k1 * t
        private static Vector2? FindInterSection(Vector2 p0, Vector2 k0, Vector2 p1, Vector2 k1)
        {
            if (Mathf.Abs(k1.x) < Mathf.Epsilon)
            {
                if (Mathf.Abs(k0.x) < Mathf.Epsilon)
                {
                    return null;
                }
                return FindInterSection(p1, k1, p0, k0);
            }
            Vector2 R = p0 - p1;
            float d = k1.y / k1.x;
            float z = (d * k0.x - k0.y);
            if (Mathf.Abs(z) < Mathf.Epsilon)
            {
                return null;
            }

            float t = (R.y - d * R.x) / z;
            return p0 + t * k0;
        }


        public static float Cross(this Vector2 u, Vector2 v)
        {
            return u.x * v.y - u.y * v.x;
        }

        public static float Dot(this Vector2 u, Vector2 v)
        {
            return Vector2.Dot(u, v);
        }

        public static bool IsZero(this float f)
        {
            return Mathf.Abs(f) < 0.00001f;
        }

        public static bool IsAlmost(this float f1, float f2)
        {
            return Mathf.Abs(f1-f2) < 0.00001f;
        }


        /// <summary>
        /// Test whether two line segments intersect. If so, calculate the intersection point.
        /// <see cref="http://stackoverflow.com/a/14143738/292237"/>
        /// </summary>
        /// <param name="p0">Vector to the start point of p.</param>
        /// <param name="p1">Vector to the end point of p.</param>
        /// <param name="q0">Vector to the start point of q.</param>
        /// <param name="q1">Vector to the end point of q.</param>
        /// <param name="intersection">The point of intersection, if any.</param>
        /// <param name="considerOverlapAsIntersect">Do we consider overlapping lines as intersecting?
        /// </param>
        /// <returns>True if an intersection point was found.</returns>

        public static bool LineSegmentsIntersect(Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1,
            out Vector2 intersection, out float ratioP, out float ratioQ)//, bool considerCollinearOverlapAsIntersect = false)
        {
            var r = p1 - p0;
            var s = q1 - q0;
            var rxs = r.Cross(s);
            var q_p = (q0 - p0);
            var qpxr = q_p.Cross(r);

//#if UNITY_EDITOR
//        Vector3 A0, A1, B0, B1;

//        p0.ToXZ(out A0);
//        p1.ToXZ(out A1);
//        q0.ToXZ(out B0);
//        q1.ToXZ(out B1);

//        Debug.DrawLine(A0,A1, Color.cyan,10);
//        Debug.DrawLine(B0,B1, Color.magenta, 10);
//#endif

            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (rxs.IsZero() && qpxr.IsZero())
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                //if (considerCollinearOverlapAsIntersect)
                //    if ((0 <= (q - p).Dot(r) && (q - p).Dot(r) <= r.Dot(r)) || (0 <= (p - q).Dot(s) && (p - q).Dot(s) <= s.Dot(s)))
                //        return true;

                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                ratioP = ratioQ = -1;
                intersection = Vector2.zero;
                return false;
            }

            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs.IsZero() && !qpxr.IsZero())
            {
                ratioP = ratioQ = -1;
                intersection = Vector2.zero;
                return false;
            }

            // t = (q - p) x s / (r x s)
            ratioP = q_p.Cross(s) / rxs;

            // u = (q - p) x r / (r x s)

            ratioQ = q_p.Cross(r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (!rxs.IsZero() && (0 <= ratioP && ratioP <= 1) && (0 <= ratioQ && ratioQ <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = p0 + ratioP * r;

                // An intersection was found.

#if UNITY_EDITOR
                Vector3 I;
                intersection.ToXZ(out I);
                DebugUtils.DrawStar(I, Color.green, 0.3f, 10);
#endif

                return true;
            }
            intersection = Vector2.zero;
            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }
        /// <summary>
        /// Returns the distance from the dot position to the segment whose origin and end points.
        /// x: the ratio on the segment line. y: the distance to the line
        /// </summary>
        /// <param name="pointPosition">the dot position</param>
        /// <param name="origin">the start position of the segment</param>
        /// <param name="end">the end position of the segment</param>
        /// <returns>in the x of the vector : the ratio on the segment line, in the y : the distance to the line</returns>
        public static Vector2 PointToLineDistance(this Vector2 pointPosition, Vector2 origin, Vector2 end)
        {
            Vector2 OE = end - origin;
            float nOE = OE.magnitude;
            if(nOE<0.000001f)
            {
                return new Vector2(0, (end-pointPosition).magnitude);
            }
            Vector2 OP = pointPosition - origin;
            Vector2 i = OE / nOE;
            float x = Vector2.Dot(OP, i);
            Vector2 I = origin + x * i;
            float y = Vector2.Distance(I, pointPosition);
            return new Vector2(x/nOE, y);
        }


        public static float PointToSegmentDistance(this Vector2 pointPosition, Vector2 origin, Vector2 end)
        {
            //Debug.DrawLine(origin, end, Color.magenta);
            Vector2 lineD = pointPosition.PointToLineDistance(origin, end);
            if(lineD.x < 0)
            {
              //  Debug.DrawLine(pointPosition, origin, Color.cyan);
                return (origin - pointPosition).magnitude;
            }
            else if(lineD.x > 1)
            {
                //Debug.DrawLine(pointPosition, end, Color.blue);
                return (end - pointPosition).magnitude;
            }
            else
            {
                //Debug.DrawLine(pointPosition, Vector2.Lerp(origin, end, lineD.x));
                return lineD.y;
            }
        }


        public static void PutAtMousePosition(this Vector3 v, Camera cam)
        {
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            v.x = r.origin.x;
            v.y = r.origin.y;
        }


        /// <summary>
        /// Lerp a vector over time, to be consistent with the Time.deltaTime threshold
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="smoothing"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Vector2 LerpOverTime(Vector2 source, Vector2 target, float smoothing, float dt)
        {
            Vector2 newValue = Vector2.zero;
            newValue.x = OSGMath.LerpOverTime(source.x, target.x, smoothing, dt);
            newValue.y = OSGMath.LerpOverTime(source.y, target.y, smoothing, dt);
            return newValue;
        }
        
        
        /// <summary>
        /// Converts a Vector2 to a Vector3.
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector, float zValue = 0f)
        {
            return new Vector3(vector.x, vector.y, zValue);
        }

        /// <summary>
        /// Retrives the vector with positive values only.
        /// </summary>
        public static Vector2 Positive(this Vector2 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);

            return vector;
        }

        /// <summary>
        /// Retrives the vector with negative values only.
        /// </summary>
        public static Vector2 Negative(this Vector2 vector)
        {
            return -vector.Positive();
        }

        /// <summary>
        /// Divides the vector with another one. Null values of otherVector will not be considered (replaced by 1).
        /// </summary>
        public static Vector2 Divide(this Vector2 vector, Vector2 otherVector)
        {
            return new Vector2(
                otherVector.x == 0f ? vector.x : vector.x / otherVector.x,
                otherVector.y == 0f ? vector.y : vector.y / otherVector.y);
        }

        /// <summary>
        /// Clamps the minimum values of the vector.
        /// </summary>
        public static Vector2 Min(this Vector2 vector, float minX, float minY)
        {
            return new Vector2(
                vector.x < minX ? minX : vector.x,
                vector.y < minY ? minY : vector.y);
        }

        /// <summary>
        /// Clamps the maximum values of the vector.
        /// </summary>
        public static Vector2 Max(this Vector2 vector, float maxX, float maxY)
        {
            return new Vector2(
                vector.x > maxX ? maxX : vector.x,
                vector.y > maxY ? maxY : vector.y);
        }

        /// <summary>
        /// Retrieves X divided by Y.
        /// </summary>
        public static float Ratio(this Vector2 vector)
        {
            return vector.x / vector.y;
        }

        /// <summary>
        /// Clamps the Vector2 in-between the given min and max vectors.
        /// </summary>
        public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(
                    Mathf.Clamp(vector.x, min.x, max.x),
                    Mathf.Clamp(vector.y, min.y, max.y));
        }

    }
}