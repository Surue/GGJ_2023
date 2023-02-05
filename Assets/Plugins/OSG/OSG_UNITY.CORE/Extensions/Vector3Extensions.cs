// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using UnityEngine;

namespace OSG
{
    public static class Vector3Extensions
    {
        public static Vector3 MostTangentAxis(this Vector3 v)
        {
            float dForward = Mathf.Abs(Vector3.Dot(v, Vector3.forward));
            float dRight = Mathf.Abs(Vector3.Dot(v, Vector3.right));
            
            float dUp = Mathf.Abs(Vector3.Dot(v, Vector3.up));
            if(dForward > dRight)
            {
                return dForward > dUp ? Vector3.forward : Vector3.up;
            }

            return dRight > dUp ? Vector3.right : Vector3.up;
        }

        public static Vector3 MostNormalAxis(this Vector3 v)
        {
            float dForward = Mathf.Abs(Vector3.Dot(v, Vector3.forward));
            float dRight = Mathf.Abs(Vector3.Dot(v, Vector3.right));
            float dUp = Mathf.Abs(Vector3.Dot(v, Vector3.up));

            if (dForward < dRight)
            {
                return dForward < dUp ? Vector3.forward : Vector3.up;
            }

            return dRight < dUp ? Vector3.right : Vector3.up;
        }

        /// <summary>
        /// Returns the angle in radians between this Vector3 and v1.
        /// Both Vector3 are copied and normalized in the function.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static float RealAngle(this Vector3 v0, Vector3 v1)
        {
            Vector3 vN0 = v0.normalized;
            Vector3 vN1 = v1.normalized;
            float dot = Vector3.Dot(vN0, vN1);
            Vector3 cross = Vector3.Cross(vN0, vN1);
            return Mathf.Atan2(cross.magnitude, dot) * Mathf.Sign(cross.y);
        }

        public static Vector2 FromXZ(this Vector3 v3)
        {
            return new Vector2(v3.x,v3.z);
        }

        public static Vector2 FromXY(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.y);
        }

        public static Vector2 FromYZ(this Vector3 v3)
        {
            return new Vector2(v3.y, v3.z);
        }

        public static Vector3 ToXZ(this Vector3 v3)
        {
            return new Vector3(v3.x, 0, v3.z);
        }

        public static Vector3 ToXY(this Vector3 v3)
        {
            return new Vector3(v3.x, v3.y, 0);
        }

        public static Vector3 ToYZ(this Vector3 v3)
        {
            return new Vector3(0, v3.y, v3.z);
        }

        public static void IntersectionXZ(ref Vector3 result, Vector3 p0, Vector3 k0, Vector3 p1, Vector3 k1)
        {
            Vector2 r = result.FromXZ();
            Vector2Extensions.Intersection(ref r, p0.FromXZ(), k0.FromXZ(), p1.FromXZ(), k1.FromXZ());
            r.ToXZ(out result, result.y);
        }

        public static Vector3? RayIntersectSphere(this Vector3 origin, Vector3 direction, Vector3 center, float radius)
        {
            Vector3 co = origin - center;
            float co2 = co.sqrMagnitude;
            float r2 = radius*radius;
            float c = co2-r2;
            float _b = -2*Vector3.Dot(direction, co); // -b, actually
            // ReSharper disable once InconsistentNaming
            float _2a = direction.sqrMagnitude * 2; // *2: it's always 2a down there, so let's do it now
            if(_2a <= float.MinValue)
                return null;

            float delta = _b*_b - 2 * _2a * c; // remember ! 
            if(delta < 0)
                return null;
            
            if(delta <= float.MinValue)
            {
                float s0 = _b/_2a;
                if(s0 < 0)
                    return null;
                return origin + s0 * direction;
            }
            delta = Mathf.Sqrt(delta);
           
            float s1 = (_b + delta)/_2a;
            float s2 = (_b - delta)/_2a;

            Vector3? solution=null;
            Action<float, float> getSolution = (k1, k2) =>
            {
                // k1 is supposed to be less than k2
                if(k1 > 0)
                    solution = origin + k1 * direction;
                else if(k2 > 0)
                    solution = origin + k2 * direction;
            };

            if(s1>s2)
                getSolution(s2, s1);
            else
                getSolution(s1, s2);

            return solution;
        }

        public static Matrix4x4 OuterProduct(this Vector3 u, Vector3 v)
        {
            Vector4 m0 = u * v[0];
            Vector4 m1 = u * v[1];
            Vector4 m2 = u * v[2];

            return new Matrix4x4(m0, m1, m2, Vector3.zero);
        }

        /// <summary>
        /// Lerp a vector over time, to be consistent with the Time.deltaTime threshold
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="smoothing"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Vector3 LerpOverTime(Vector3 source, Vector3 target, float smoothing, float dt)
        {
            Vector3 newValue = Vector3.zero;
            newValue.x = OSGMath.LerpOverTime(source.x, target.x, smoothing, dt);
            newValue.y = OSGMath.LerpOverTime(source.y, target.y, smoothing, dt);
            newValue.z = OSGMath.LerpOverTime(source.z, target.z, smoothing, dt);
            return newValue;
        }


        /// <summary>
        /// Multiplies the vector with another one.
        /// </summary>
        public static Vector3 Multiply(this Vector3 vector, Vector3 other)
        {
            return new Vector3(
                vector.x * other.x,
                vector.y * other.y,
                vector.z * other.z);
        }

        /// <summary>
        /// Divides the vector with another one. Null values of otherVector will not be considered (replaced by 1).
        /// </summary>
        public static Vector3 Divide(this Vector3 vector, Vector3 otherVector)
        {
            return new Vector3(
                otherVector.x == 0f ? vector.x : vector.x / otherVector.x,
                otherVector.y == 0f ? vector.y : vector.y / otherVector.y,
                otherVector.z == 0f ? vector.z : vector.z / otherVector.z);
        }


        /// <summary>
        /// Snaps the vector to the given Vector3 spacing.
        /// </summary>
        public static Vector3 Snap(this Vector3 vector, Vector3 snap, Vector3 offset = default)
        {
            return new Vector3(
                Mathf.RoundToInt((vector.x + offset.x) / snap.x) * snap.x,
                Mathf.RoundToInt((vector.y + offset.y) / snap.y) * snap.y,
                Mathf.RoundToInt((vector.z + offset.z) / snap.z) * snap.z) - offset;
        }


        /// <summary>
        /// Returns the vector with the specified X value.
        /// </summary>
        public static Vector3 XValue(this Vector3 vector, float xValue)
        {
            vector.x = xValue;
            return vector;
        }

        /// <summary>
        /// Returns the vector with the specified X value.
        /// </summary>
        public static Vector3 YValue(this Vector3 vector, float yValue)
        {
            vector.y = yValue;
            return vector;
        }

        /// <summary>
        /// Returns the vector with the specified X value.
        /// </summary>
        public static Vector3 ZValue(this Vector3 vector, float zValue)
        {
            vector.z = zValue;
            return vector;
        }


        /// <summary>
        /// Returns the vector with offseted X value.
        /// </summary>
        public static Vector3 XOffset(this Vector3 vector, float xOffset)
        {
            vector.x += xOffset;
            return vector;
        }

        /// <summary>
        /// Returns the vector with offseted Y value.
        /// </summary>
        public static Vector3 YOffset(this Vector3 vector, float yOffset)
        {
            vector.y += yOffset;
            return vector;
        }

        /// <summary>
        /// Returns the vector with offseted Z value.
        /// </summary>
        public static Vector3 ZOffset(this Vector3 vector, float zOffset)
        {
            vector.z += zOffset;
            return vector;
        }


        /// <summary>
        /// Retrives the vector with positive values only.
        /// </summary>
        public static Vector3 Positive(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);

            return vector;
        }

        /// <summary>
        /// Retrives the vector with negative values only.
        /// </summary>
        public static Vector3 Negative(this Vector3 vector)
        {
            return -vector.Positive();
        }


        /// <summary>
        /// Clamps the minimum values of the vector.
        /// </summary>
        public static Vector3 Min(this Vector3 vector, float minX, float minY, float minZ)
        {
            return new Vector3(
                vector.x < minX ? minX : vector.x,
                vector.y < minY ? minY : vector.y,
                vector.z < minZ ? minZ : vector.z);
        }

        /// <summary>
        /// Clamps the maximum values of the vector.
        /// </summary>
        public static Vector3 Max(this Vector3 vector, float maxX, float maxY, float maxZ)
        {
            return new Vector3(
                vector.x > maxX ? maxX : vector.x,
                vector.y > maxY ? maxY : vector.y,
                vector.z > maxZ ? maxZ : vector.z);
        }

        /// <summary>
        /// Clamps the Vector3 in-between the given min and max vectors.
        /// </summary>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                    Mathf.Clamp(vector.x, min.x, max.x),
                    Mathf.Clamp(vector.y, min.y, max.y),
                    Mathf.Clamp(vector.z, min.z, max.z));
        }
    }
}