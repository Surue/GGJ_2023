// Old Skull Games
// Pierre Planeau
// Thursday, February 22, 2018

using UnityEngine;
using OSG.Core;
using System;

namespace OSG
{
    public static partial class OSGMath
    {
        /// <summary>
        /// Linearly interpolates between two vectors.
        /// 't' ranges from 0 to 1000.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="t">Range: [0, 1000]</param>
        /// <returns></returns>
        public static Vector3Int Vector3IntLerp(Vector3Int v0, Vector3Int v1, int t)
        {
            t = CoreMath.Clamp(t, 0, 1000);
            Vector3Int tmp = v0 * (1000 - t) + (v1 * t);
            tmp.x /= 1000;
            tmp.y /= 1000;
            tmp.z /= 1000;
            return tmp;
        }

        public static Vector3Int normalized(this Vector3Int v)
        {
            int magnitude = v.IntMagnitude();
            v = v * 1000;

            v.x /= magnitude;
            v.y /= magnitude;
            v.z /= magnitude;

            return v;
        }

        public static Vector3Int Vector3IntNormalize(Vector3Int v)
        {
            Vector3Int v2 = new Vector3Int(v.x, v.y, v.z);
            return v2.normalized();
        }

        public static int IntDistance(Vector3Int v1, Vector3Int v2)
        {
            //return IntSqrt(((v2.x - v1.x) * (v2.x - v1.x)) + ((v2.y - v1.y) * (v2.y - v1.y)) + ((v2.z - v1.z) * (v2.z - v1.z)));
            long x = v2.x - v1.x;
            long y = v2.y - v1.y;
            long z = v2.z - v1.z;
            return (int)CoreMath.LongSqrt(x * x + y * y + z * z);
        }

        public static int IntMagnitude(this Vector3Int v)
        {
            long x = v.x;
            long y = v.y;
            long z = v.z;
            return (int)CoreMath.LongSqrt(x * x + y * y + z * z);
        }

        public static int Vector3IntDot(Vector3Int v0, Vector3Int v1)
        {
            return (v0.x * v1.x) + (v0.y * v1.y) + (v0.z * v1.z);
        }

        public static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
        }


        /// <summary>
        /// Project a vector on a line, so the new vector fit the line direction
        /// </summary>
        /// <param name="lineDir"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 ProjectVectorOnLine(this Vector2 vector,Vector2 lineDir)
        {
            return (Vector2.Dot(lineDir, vector) * lineDir);
        }

        /// <summary>
        /// Project a vector on a line, so the new vector fit the line direction
        /// </summary>
        /// <param name="lineDir"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 ProjectVectorOnLine(this Vector3 vector, Vector3 lineDir)
        {
            return (Vector3.Dot(lineDir,vector)) * lineDir;
        }


        /// <summary>
        /// Return the rejection vector (vector from vector2 to vector1) (https://en.wikipedia.org/wiki/Vector_projection))
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="lineDir"></param>
        /// <returns></returns>
        public static Vector2 VectorRejection(this Vector2 vector1, Vector2 vector2)
        {
            return vector1 - Vector2.Dot(vector2,vector1)/Vector2.Dot(vector2, vector2)*vector2;
        }


        /// <summary>
        /// Return the rejection vector (vector from vector2 to vector1) (https://en.wikipedia.org/wiki/Vector_projection))
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="lineDir"></param>
        /// <returns></returns>
        public static Vector3 VectorRejection(this Vector3 vector1, Vector3 vector2)
        {
            return vector1 - Vector3.Dot(vector2, vector1) / Vector3.Dot(vector2, vector2) * vector2;
        }

        /// <summary>
        /// Return the projected point on a line. The projected point is the point on line that minimise the distance between him and the point parameter
        /// </summary>
        /// <param name="lineDir"></param>
        /// <param name="linePoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 ProjectPointOnLine(this Vector2 point, Vector2 lineDir, Vector2 linePoint)
        {
            float t = Vector2.Dot(point - linePoint, lineDir);
            return linePoint + lineDir * t;
        }


        /// <summary>
        /// Return the projected point on a line. The projected point is the point on line that minimise the distance between him and the point parameter
        /// </summary>
        /// <param name="lineDir"></param>
        /// <param name="linePoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ProjectPointOnLine(this Vector3 point, Vector3 lineDir, Vector3 linePoint)
        {
            float t = Vector3.Dot(point - linePoint, lineDir);
            return linePoint + lineDir * t;
        }



        //Get the shortest distance between a point and a line. The output is signed so it holds information
        public static float DistancePointToLine(this Vector2 point, Vector2 lineDir, Vector2 linePoint)
        {
            Vector2 vector = linePoint - point;

            return Mathf.Abs(vector.x * lineDir.y - vector.y * lineDir.x);
        }


        /// <summary>
        /// create a vector of direction "vector" with length "size"
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector2 SetVectorLength(this Vector2 vector, float size)
        {
            //normalize the vector
            return vector.normalized * size;
        }

        /// <summary>
        /// create a vector of direction "vector" with length "size"
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 SetVectorLength(this Vector3 vector, float size)
        {
            //normalize the vector
            return vector.normalized * size;
        }


        /// <summary>
        /// Check if a point is on a segment
        /// </summary>
        /// <param name="point"></param>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <returns></returns>
        public static bool PointIsOnLine(this Vector2 point, Vector2 lineDir, Vector2 linePoint, float threshold = 0.01f)
        {
            return AreCollinear(point, linePoint, linePoint + lineDir, threshold);
        }

        /// <summary>
        /// Check if a point is on a segment
        /// </summary>
        /// <param name="point"></param>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <returns></returns>
        public static bool PointIsOnSegment(this Vector2 point, Vector2 firstPoint, Vector2 secondPoint, float threshold = 0.01f)
        {
            //Cross product tell us if all point are on the same line
            if (AreCollinear(point, firstPoint, secondPoint, threshold))
            {
                Vector2 dir1 = firstPoint - point;
                Vector2 dir2 = secondPoint - point;
                //Dot product tell us if vector are or not oriented on the same way (with the sign)
                return Vector2.Dot(dir1, dir2) < 0;
            }

            return false;
        }


        static bool AreCollinear(Vector2 v1, Vector2 v2, Vector3 v3, float threshold = 0.01f)
        {

            /* Calculation the area of   
            triangle. We have skipped  
            multiplication with 0.5 to  
            avoid floating point computations */

            //DET product
            float a = v1.x * (v2.y - v3.y) +
                    v2.x * (v3.y - v1.y) +
                    v3.x * (v1.y - v2.y);

            if (a <= threshold)
                return true;
            else
                return false;
        }



        /// <summary>
        /// Returns a point which is a projection from a point to a line segment.
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ProjectPointOnLineSegment(this Vector3 point, Vector3 firstPoint, Vector3 secondPoint)
        {
            Vector3 segmentDir = secondPoint - firstPoint;
            float segmentLength2 = segmentDir.sqrMagnitude;
            

            float t = Vector3.Dot(point - firstPoint, segmentDir);

            //point is on side of linePoint2, compared to linePoint1
            if (t > 0)
            {
                //point is on the line segment
                if (t <= segmentLength2)
                {
                    return firstPoint + segmentDir * t / segmentLength2;
                }
                //point is not on the line segment and it is on the side of linePoint2
                else
                {
                    return secondPoint;
                }
            }
            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else
            {
                return firstPoint;
            }
        }

        /// <summary>
        /// Returns a point which is a projection from a point to a line segment.
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 ProjectPointOnLineSegment(this Vector2 point, Vector2 firstPoint, Vector2 secondPoint)
        {
            Vector2 segmentDir = secondPoint - firstPoint;
            float segmentLength2 = segmentDir.sqrMagnitude;


            float t = Vector2.Dot(point - firstPoint, segmentDir);

            //point is on side of linePoint2, compared to linePoint1
            if (t > 0)
            {
                //point is on the line segment
                if (t <= segmentLength2)
                {
                    return firstPoint + segmentDir * t/ segmentLength2;
                }
                //point is not on the line segment and it is on the side of linePoint2
                else
                {
                    return secondPoint;
                }
            }
            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else
            {
                return firstPoint;
            }
        }



        /// <summary>
        /// Returns a point which is a projection from a point to a plane.
        /// </summary>
        /// <param name="planeNormal"></param>
        /// <param name="planePoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ProjectPointOnPlane(this Vector3 point, Vector3 planeNormal, Vector3 planePoint)
        {

            float distance;
            Vector3 translationVector;

            //First calculate the distance from the point to the plane:
            distance = SignedDistancePlanePoint(point, planeNormal, planePoint);

            //Reverse the sign of the distance
            distance *= -1;

            //Get a translation vector
            translationVector = SetVectorLength(planeNormal, distance);

            //Translate the point to form a projection
            return point + translationVector;
        }

        //Projects a vector onto a plane. The output is not normalized.
        public static Vector3 ProjectVectorOnPlane(this Vector3 vector, Vector3 planeNormal)
        {

            return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
        }

        /// <summary>
        /// Get the intersection between a line and a plane. 
        ///If the line and plane are not parallel, the function outputs true, otherwise false.
        ///From : https://wiki.unity3d.com/index.php/3d_Math_functions
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="linePoint"></param>
        /// <param name="lineVec">As to be normalized</param>
        /// <param name="planeNormal"></param>
        /// <param name="planePoint"></param>
        /// <returns></returns>
        public static Vector3? LinePlaneIntersection( Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {

            float length;
            float dotNumerator;
            float dotDenominator;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //get the coordinates of the line-plane intersection point
               return linePoint + lineVec * length;
            }
            //output not valid
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Get the intersection between a line and a plane. 
        ///If the line and plane are not parallel, the function outputs true, otherwise false.
        ///From : https://wiki.unity3d.com/index.php/3d_Math_functions
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="linePoint"></param>
        /// <param name="lineVec">As to be normalized</param>
        /// <param name="planeNormal"></param>
        /// <param name="planePoint"></param>
        /// <returns></returns>
        public static Vector3? SegmentPlaneIntersection( Vector3 segmentPoint1, Vector3 segmentPoint2, Vector3 planeNormal, Vector3 planePoint)
        {
            float dotNumerator;
            float dotDenominator;
            Vector3 segmentDir = segmentPoint2 - segmentPoint1;
            float segmentLength = segmentDir.magnitude;
            segmentDir = segmentDir/ segmentLength;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector3.Dot((planePoint - segmentPoint1), planeNormal);
            dotDenominator = Vector3.Dot(segmentDir, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                float length = dotNumerator / dotDenominator;
                if (length >= 0 && length  <= segmentLength)
                {
                    //get the coordinates of the line-plane intersection point
                    return segmentPoint1 + segmentDir * length;
                }
                else
                    return null;
            }
            //output not valid
            else
            {
                return null;
            }
        }

        /// <summary>
        ///Get the shortest distance between a point and a plane. The output is signed so it holds information
        ///as to which side of the plane normal the point is.
        /// </summary>
        /// <param name="planeNormal"></param>
        /// <param name="planePoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float SignedDistancePlanePoint(this Vector3 point, Vector3 planeNormal, Vector3 planePoint)
        {
            return Vector3.Dot(planeNormal, (planePoint - point));
        }

        /// <summary>
        /// Return the point intersection of 2 line
        /// Can return null if line are parallel
        /// </summary>
        /// <param name="linePoint1"></param>
        /// <param name="lineDir1"></param>
        /// <param name="linePoint2"></param>
        /// <param name="lineDir2"></param>
        /// <returns></returns>
        public static Vector2? LineLineIntersection(Vector2 linePoint1, Vector2 lineDir1, Vector2 linePoint2, Vector2 lineDir2)
        {
            float delta =  lineDir2.y * lineDir1.x - lineDir1.y * lineDir2.x;

            //If lines are parallel, the result will be (NaN, NaN).
            if (delta == 0)
                return null;
            else
            {
                float c1 = lineDir1.y * linePoint1.x - lineDir1.x * linePoint1.y;
                float c2 = lineDir2.y * linePoint2.x - lineDir2.x * linePoint2.y;
                return new Vector2((-lineDir2.x * c1 + lineDir1.x * c2) / delta, (lineDir1.y * c2 - lineDir2.y * c1) / delta);
            }
        }


        /// <summary>
        /// Return the point intersection of 2 line, From : https://wiki.unity3d.com/index.php/3d_Math_functions
        /// Can return null if line are parallel
        /// </summary>
        /// <param name="linePoint1"></param>
        /// <param name="lineDir1"></param>
        /// <param name="linePoint2"></param>
        /// <param name="lineDir2"></param>
        /// <returns></returns>
        public static Vector3? LineLineIntersection(Vector3 linePoint1, Vector3 lineDir1, Vector3 linePoint2, Vector3 lineDir2)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineDir1, lineDir2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDir2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parrallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                return linePoint1 + (lineDir1 * s);
            }
            else
            {
                return null;
            }

        }


        public static bool SegmentIntersecting(Vector2 firstPoint1, Vector2 secondPoint1, Vector2 firstPoint2, Vector2 secondPoint2, float threshold = 0.01f)
        {
            Vector2? intersectPoint = LineLineIntersection(firstPoint1, (firstPoint1 - secondPoint1).normalized, firstPoint2, (firstPoint2 - secondPoint2).normalized);

            if (intersectPoint == null)
            {
                return false;
            }
            else
            {
                Vector2 dir1 = firstPoint1 - (Vector2)intersectPoint;
                Vector2 dir2 = secondPoint1 - (Vector2)intersectPoint;

                //Dot product tell us if vector are or not oriented on the same way (with the sign)
                if (!(Vector2.Dot(dir1, dir2) < 0))
                    return false;

                dir1 = firstPoint2 - (Vector2)intersectPoint;
                dir2 = secondPoint2 - (Vector2)intersectPoint;

                return (Vector2.Dot(dir1, dir2) < 0);
            }
        }

        /// <summary>
        /// Return the ClosestPoints of line 1 and . From :http://geomalgorithms.com/a07-_distance.html
        /// </summary>
        /// <param name="linePoint1"></param>
        /// <param name="lineDir1"></param>
        /// <param name="linePoint2"></param>
        /// <param name="lineDir2"></param>
        /// <returns></returns>
        public static Tuple<Vector3,Vector3> LineLineClosestPoints(Vector3 linePoint1, Vector3 lineDir1, Vector3 linePoint2, Vector3 lineDir2)
        {
            float a = Vector3.Dot(lineDir1, lineDir1);
            float b = Vector3.Dot(lineDir1, lineDir2);
            float c = Vector3.Dot(lineDir2, lineDir2);

            float ac_minus_b2 = a * c - b * b;

            if (ac_minus_b2 == 0)
                return null;
            else
            {
                Vector3 w0 = linePoint1 - linePoint2;
                float d = Vector3.Dot(lineDir1, w0);
                float e = Vector3.Dot(lineDir2, w0);

                float t1 = (b * e - c * d) / (ac_minus_b2);
                float t2 = (a * e - b * d) / (ac_minus_b2);

                Vector3 pointOn1 = linePoint1 + t1 * lineDir1;
                Vector3 pointOn2 = linePoint2 + t2 * lineDir2;

                return new Tuple<Vector3, Vector3>(pointOn1, pointOn2);
            }
        }


        /// <summary>
        /// Exact Lerp aware of deltaTime threshold
        /// Can be costly
        /// Source : http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/?fbclid=IwAR2ABNkxmTADiqZJgdGH9uKtnpjI6i91cIrwUPqY0DF3agET_yIK_0U5x0Q
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="smoothing"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static float LerpOverTime(float a, float b, float lambda, float dt)
        {
            return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }


        #region Equation

        /// <summary>
        /// n^(1/3) - work around a negative double raised to (1/3)
        /// </summary>
        static float PowThird(float n)
        {
            return Mathf.Pow(Math.Abs(n), 1f / 3f) * Math.Sign(n);
        }

        /// <summary>
        /// Q and R are transformed variables.
        /// </summary>
        static void QR(float a2, float a1, float a0, out float Q, out float R)
        {
            float a22 = a2 * a2;
            Q = (3 * a1 - a22) / 9.0f;
            R = (9.0f * a2 * a1 - 27 * a0 - 2 * a2 * a22) / 54.0f;
        }

        /// <summary>
        /// Find all real-valued roots of the cubic equation a0 + a1*x + a2*x^2 + x^3 = 0.
        /// Note the special coefficient order ascending by exponent (consistent with polynomials).
        /// From https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/RootFinding/Cubic.cs
        /// </summary>
        public static Tuple<float, float, float> RealRoots(float a0, float a1, float a2)
        {
            float Q, R;
            QR(a2, a1, a0, out Q, out R);

            float Q3 = Q * Q * Q;
            float D = Q3 + R * R;
            float shift = -a2 / 3f;

            float x1;
            float x2 = float.NaN;
            float x3 = float.NaN;

            if (D >= 0)
            {
                // when D >= 0, use eqn (54)-(56) where S and T are real
                float sqrtD = Mathf.Pow(D, 0.5f);
                float S = PowThird(R + sqrtD);
                float T = PowThird(R - sqrtD);
                x1 = shift + (S + T);
                if (D == 0)
                {
                    x2 = shift - S;
                }
            }
            else
            {
                // 3 real roots, use eqn (70)-(73) to calculate the real roots
                float theta = Mathf.Acos(R / Mathf.Sqrt(-Q3));
                x1 = 2f * Mathf.Sqrt(-Q) * Mathf.Cos(theta / 3.0f) + shift;
                x2 = 2f * Mathf.Sqrt(-Q) * Mathf.Cos((theta + 2.0f * Mathf.PI) / 3f) + shift;
                x3 = 2f * Mathf.Sqrt(-Q) * Mathf.Cos((theta - 2.0f * Mathf.PI) / 3f) + shift;
            }

            return new Tuple<float, float, float>(x1, x2, x3);
        }
        #endregion
    }
}
