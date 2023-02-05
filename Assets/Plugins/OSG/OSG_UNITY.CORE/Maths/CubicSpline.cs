// Old Skull Games
// Pierre Planeau
// Wednesday, February 21, 2018

using UnityEngine;
using OSG.Core;

namespace OSG
{
    /// <summary>
    /// Cubic Hermite Spline
    /// https://en.wikipedia.org/wiki/Cubic_Hermite_spline
    /// </summary>
    public class SimpleCubicSpline
    {
        protected Vector3 P0, P1;
        protected Vector3 T0, T1;

        protected Vector3 cache_calcA;
        protected Vector3 cache_calcB;

        public int Length { get { return 2; } }

        public Vector3 start
        {
            get { return P0; }
            set { 
                P0 = value;
                CacheValues();
            }
        }

        public Vector3 end
        {
            get { return P1; }
            set { 
                P1 = value;
                CacheValues();
            }
        }

        public Vector3 startTangent
        {
            get { return T0; }
            set
            {
                T0 = value;
                CacheValues();
            }
        }

        public Vector3 endTangent
        {
            get { return T1; }
            set
            {
                T1 = value;
                CacheValues();
            }
        }

        public SimpleCubicSpline(Vector3 P0, Vector3 P1, Vector3 T0, Vector3 T1)
        {
            Set(P0, P1, T0, T1);
        }

        public void Set(Vector3 P0, Vector3 P1, Vector3 T0, Vector3 T1)
        {
            this.P0 = P0;
            this.P1 = P1;
            this.T0 = T0;
            this.T1 = T1;

            CacheValues();
        }

        private void CacheValues()
        {
            cache_calcA = (2 * (P0 - P1) + T0 + T1);
            cache_calcB = (3 * (P1 - P0) - 2 * T0 - T1);
        }

        /// <summary>
        /// Interpolate the Cubic Spline described by the two points and two tangents, using t [0, 1].
        /// </summary>
        /// <param name="firstPointIndex"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Interpolate(float t)
        {
            return t * (t * (t * cache_calcA + cache_calcB) + T0) + P0;
        }

        public Vector3 SplineTangent(float t)
        {
            return t * (t * (3 * cache_calcA) + (2 * cache_calcB)) + T0;
        }


        /// <summary>
        /// Interpolates the Cubic Spline described by the two points and two tangents at t [0, 1].
        /// </summary>
        /// <param name="P0"></param>
        /// <param name="P1"></param>
        /// <param name="T0"></param>
        /// <param name="T1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Spline(Vector3 P0, Vector3 P1, Vector3 T0, Vector3 T1, float t)
        {
            /*return (2 * (P0 - P1) + T0 + T1) * (t * t * t) +
                   (3 * (P1 - P0) - 2 * T0 - T1) * (t * t) +
                   T0 * t +
                   P0;*/
            return t * (t * (t * (2 * (P0 - P1) + T0 + T1) + (3 * (P1 - P0) - 2 * T0 - T1)) + T0) + P0;
        }
    }

    public class SimpleCubicSpline2D
    {
        protected Vector2 P0, P1;
        protected Vector2 T0, T1;

        protected Vector2 cache_calcA;
        protected Vector2 cache_calcB;


        public void Set(Vector2 P0, Vector2 P1, Vector2 T0, Vector2 T1)
        {
            this.P0 = P0;
            this.P1 = P1;
            this.T0 = T0;
            this.T1 = T1;

            cache_calcA = (2 * (P0 - P1) + T0 + T1);
            cache_calcB = (3 * (P1 - P0) - 2 * T0 - T1);
        }

        public SimpleCubicSpline2D(Vector2 P0, Vector2 P1, Vector2 T0, Vector2 T1)
        {
            Set(P0,P1,T0,T1);
        }

        /// <summary>
        /// Interpolate the Cubic Spline described by the two points and two tangents, using t [0, 1].
        /// </summary>
        /// <param name="firstPointIndex"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector2 Interpolate(float t)
        {
            return t * (t * (t * cache_calcA + cache_calcB) + T0) + P0;
        }

        public Vector2 SplineTangent(float t)
        {
            return t * (t * (3 * cache_calcA) + (2 * cache_calcB)) + T0;
        }


        /// <summary>
        /// Interpolates the Cubic Spline described by the two points and two tangents at t [0, 1].
        /// </summary>
        /// <param name="P0"></param>
        /// <param name="P1"></param>
        /// <param name="T0"></param>
        /// <param name="T1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 Spline(Vector2 P0, Vector2 P1, Vector2 T0, Vector2 T1, float t)
        {
            /*return (2 * (P0 - P1) + T0 + T1) * (t * t * t) +
                   (3 * (P1 - P0) - 2 * T0 - T1) * (t * t) +
                   T0 * t +
                   P0;*/
            return t * (t * (t * (2 * (P0 - P1) + T0 + T1) + (3 * (P1 - P0) - 2 * T0 - T1)) + T0) + P0;
        }
    }


    public class CubicSpline
    {
        protected Vector3[] points;
        protected Vector3[] tangents;

        protected Vector3[] cache_calcA;
        protected Vector3[] cache_calcB;

        public int Length { get { return points.Length; } }


        public CubicSpline(Vector3[] points, Vector3[] tangents)
        {
            int pointsLength = points.Length;

            if (pointsLength < 2)
            {
                throw new System.Exception("There must be at least 2 points to create a CubicSpline.");
            }
            if (pointsLength != tangents.Length)
            {
                throw new System.Exception("Points and Tangents must be of same size");
            }

            this.points = points;
            this.tangents = tangents;

            int cache_Length = pointsLength - 1;
            cache_calcA = new Vector3[cache_Length];
            cache_calcB = new Vector3[cache_Length];

            for (int i = 0; i < cache_Length; ++i)
            {
                Vector3 P0 = points[i];
                Vector3 P1 = points[i + 1];
                Vector3 T0 = tangents[i];
                Vector3 T1 = tangents[i + 1];

                cache_calcA[i] = (2 * (P0 - P1) + T0 + T1);
                cache_calcB[i] = (3 * (P1 - P0) - 2 * T0 - T1);
            }
        }

        /// <summary>
        /// Interpolate the Cubic Spline described by the point at index 'firstPointIndex' and the next point, using t [0, 1].
        /// </summary>
        /// <param name="firstPointIndex"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Interpolate(int firstPointIndex, float t)
        {
            return t * (t * (t * cache_calcA[firstPointIndex] + cache_calcB[firstPointIndex]) + tangents[firstPointIndex]) + points[firstPointIndex];

            //return t * (t * (t * (2 * (P0 - P1) + T0 + T1) + (3 * (P1 - P0) - 2 * T0 - T1)) + T0) + P0;
            //                     (      cache_calcA      )   (        cache_calcB        )
        }

        public Vector3 SplineTangent(int firstPointIndex, float t)
        {
            return t * (t * (3 * cache_calcA[firstPointIndex]) + (2 * cache_calcB[firstPointIndex])) + tangents[firstPointIndex];

            //return (3 * cache_calcA[firstPointIndex] * t * t) + (2 * cache_calcB[firstPointIndex] * t) + tangents[firstPointIndex];
        }


        /// <summary>
        /// Interpolates the Cubic Spline described by the two points and two tangents at t [0, 1].
        /// </summary>
        /// <param name="P0"></param>
        /// <param name="P1"></param>
        /// <param name="T0"></param>
        /// <param name="T1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Spline(Vector3 P0, Vector3 P1, Vector3 T0, Vector3 T1, float t)
        {
            /*return (2 * (P0 - P1) + T0 + T1) * (t * t * t) +
                   (3 * (P1 - P0) - 2 * T0 - T1) * (t * t) +
                   T0 * t +
                   P0;*/
            return t * (t * (t * (2 * (P0 - P1) + T0 + T1) + (3 * (P1 - P0) - 2 * T0 - T1)) + T0) + P0;
        }
    }

    public class CubicSplineInt
    {
        public const int rangeMax = 1000;

        protected Vector3Int[] points;
        protected Vector3Int[] tangents;

        protected Vector3Int[] cache_calcA;
        protected Vector3Int[] cache_calcB;

        public int Length { get { return points.Length; } }


        public CubicSplineInt(Vector3Int[] points, Vector3Int[] tangents)
        {
            int pointsLength = points.Length;

            if (pointsLength < 2)
            {
                throw new System.Exception("There must be at least 2 points to create a CubicSpline.");
            }
            if (pointsLength != tangents.Length)
            {
                throw new System.Exception("Points and Tangents must be of same size");
            }

            this.points = points;
            this.tangents = tangents;

            int cache_Length = pointsLength - 1;
            cache_calcA = new Vector3Int[cache_Length];
            cache_calcB = new Vector3Int[cache_Length];

            for (int i = 0; i < cache_Length; ++i)
            {
                Vector3Int P0 = points[i];
                Vector3Int P1 = points[i + 1];
                Vector3Int T0 = tangents[i];
                Vector3Int T1 = tangents[i + 1];

                cache_calcA[i] = ((P0 - P1) * 2 + T0 + T1);
                cache_calcB[i] = ((P1 - P0) * 3 - T0 * 2 - T1);
            }
        }

        /// <summary>
        /// Interpolate the Cubic Spline described by the point at index 'firstPointIndex' and the next point, using t [0, 1000].
        /// </summary>
        /// <param name="firstPointIndex"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3Int Interpolate(int firstPointIndex, int t)
        {
            t = CoreMath.Clamp(t, 0, rangeMax);

            Vector3Int tmp1 = cache_calcA[firstPointIndex] * t;
            tmp1.x /= rangeMax;
            tmp1.y /= rangeMax;
            tmp1.z /= rangeMax;
            Vector3Int tmp2 = (tmp1 + cache_calcB[firstPointIndex]) * t;
            tmp2.x /= rangeMax;
            tmp2.y /= rangeMax;
            tmp2.z /= rangeMax;
            Vector3Int tmp3 = (tmp2 + tangents[firstPointIndex]) * t;
            tmp3.x /= rangeMax;
            tmp3.y /= rangeMax;
            tmp3.z /= rangeMax;

            return tmp3 + points[firstPointIndex];

            //return t * (t * (t * (2 * (P0 - P1) + T0 + T1) + (3 * (P1 - P0) - 2 * T0 - T1)) + T0) + P0;
            //                     (      cache_calcA      )   (        cache_calcB        )
        }

        public Vector3Int SplineTangent(int firstPointIndex, int t)
        {
            t = CoreMath.Clamp(t, 0, rangeMax);

            Vector3Int tmp1 = cache_calcA[firstPointIndex] * 3 * t;
            tmp1.x /= rangeMax;
            tmp1.y /= rangeMax;
            tmp1.z /= rangeMax;
            Vector3Int tmp2 = (tmp1 + (cache_calcB[firstPointIndex] * 2)) * t;
            tmp2.x /= rangeMax;
            tmp2.y /= rangeMax;
            tmp2.z /= rangeMax;

            return tmp2 + tangents[firstPointIndex];
        }


        /// <summary>
        /// Interpolates the Cubic Spline described by the two points and two tangents at t [0, 1000].
        /// </summary>
        /// <param name="P0"></param>
        /// <param name="P1"></param>
        /// <param name="T0"></param>
        /// <param name="T1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3Int Spline(Vector3Int P0, Vector3Int P1, Vector3Int T0, Vector3Int T1, int t)
        {
            t = CoreMath.Clamp(t, 0, rangeMax);

            Vector3Int tmp1 = ((P0 - P1) * 2 + T0 + T1) * t;
            tmp1.x /= rangeMax;
            tmp1.y /= rangeMax;
            tmp1.z /= rangeMax;
            Vector3Int tmp2 = (tmp1 + ((P1 - P0) * 3 - T0 * 2 - T1)) * t;
            tmp2.x /= rangeMax;
            tmp2.y /= rangeMax;
            tmp2.z /= rangeMax;
            Vector3Int tmp3 = (tmp2 + T0) * t;
            tmp3.x /= rangeMax;
            tmp3.y /= rangeMax;
            tmp3.z /= rangeMax;

            return tmp3 + P0;
        }
    }
}
