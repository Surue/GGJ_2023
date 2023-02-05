using UnityEngine;

namespace OSG
{

    public static class CatmullRom2D
    {
        /// <summary>
        /// Get a point following the spline passing by the four control points, between p1 and p2, using a linear interpolation [0f, 1f].
        /// https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
        /// </summary>
        /// <param name="p0">First control point of the spline</param>
        /// <param name="p1">Point at which the interpolation begins</param>
        /// <param name="p2">Point at which the interpolation ends</param>
        /// <param name="p3">Last control point of the spline</param>
        /// <param name="t">Linear interpolation paramater (between 0 and 1)</param>
        /// <returns></returns>
        public static Vector2 Get(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            /*
        return Vector2.Lerp(p1, p2, t);
        /*/
            return 0.5f * (2f * p1 + t * (p2 - p0 + t * (2f * p0 - 5f * p1 + 4f * p2 - p3 + t * (3f * (p1 - p2) + p3 - p0))));
            //*/
        }

        public static float Get(float p0, float p1, float p2, float p3, float t)
        {
            /*
        return Vector2.Lerp(p1, p2, t);
        /*/
            return 0.5f * (2f * p1 + t * (p2 - p0 + t * (2f * p0 - 5f * p1 + 4f * p2 - p3 + t * (3f * (p1 - p2) + p3 - p0))));
            //*/
        }


        public static Vector2 Get(ref Vector2 p0, ref Vector2 p1, ref Vector2 p2, ref Vector2 p3, float t)
        {
            Vector2 res =  0.5f * (2f * p1 + t * (p2 - p0 + t * (2f * p0 - 5f * p1 + 4f * p2 - p3 + t * (3f * (p1 - p2) + p3 - p0))));
            return res;
        }

        public static Vector2 GetFirstDerivative(ref Vector2 P0, ref Vector2 P1, ref Vector2 P2, ref Vector2 P3, float t)
        {
            Vector2 res = 0.5f*((-P0 + P2) +
                                2*(2*P0 - 5*P1 + 4*P2 - P3)*t +
                                3*(-P0 + 3*P1 - 3*P2 + P3)*t*t);
            return res;
        }

        public static Vector2 GetSecondDerivative(ref Vector2 P0, ref Vector2 P1, ref Vector2 P2, ref Vector2 P3, float t)
        {
            return 0.5f*(2*(2*P0 - 5*P1 + 4*P2 - P3) +
                         6*(-P0 + 3*P1 - 3*P2 + P3)*t);
        }

        public static float WorldDistanceToSplineDistance(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 P3, float t, float worldDistance)
        {
            Vector2 derivative = GetFirstDerivative(ref P0, ref P1, ref P2, ref P3, t);
            float dt = derivative.magnitude;
            if (Mathf.Abs(dt) < float.Epsilon)
            {
                dt = float.Epsilon * Mathf.Sign(dt);
            }
            return worldDistance / dt;
        }

        static private float sqr (float v)
        {
            return v*v;
        }

        static public float GetCurvature(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            Vector2 p = GetFirstDerivative(ref p0, ref p1, ref p2, ref p3, t); // prime
            Vector2 s = GetSecondDerivative(ref p0, ref p1, ref p2, ref p3, t); // second
            float d = p.magnitude;
            if (d < Mathf.Epsilon)
            {
                return float.MaxValue;
            }

            d = d*d*d;

            float n = Mathf.Sqrt(sqr(s.y*p.x - s.x*p.y));
            return n/d;
        }
    }

    public static class CatmullRom
    {
        /// <summary>
        /// Get a point following the spline passing by the four control points, between p1 and p2, using a linear interpolation [0f, 1f].
        /// https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
        /// </summary>
        /// <param name="p0">First control point of the spline</param>
        /// <param name="p1">Point at which the interpolation begins</param>
        /// <param name="p2">Point at which the interpolation ends</param>
        /// <param name="p3">Last control point of the spline</param>
        /// <param name="t">Linear interpolation paramater (between 0 and 1)</param>
        /// <returns></returns>
        public static Vector3 Get(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            /*
        return Vector3.Lerp(p1, p2, t);
        /*/
            return 0.5f * (2f * p1 + t * (p2 - p0 + t * (2f * p0 - 5f * p1 + 4f * p2 - p3 + t * (3f * (p1 - p2) + p3 - p0))));
            //*/
        }

        public static float Get(float p0, float p1, float p2, float p3, float t)
        {
            /*
        return Vector3.Lerp(p1, p2, t);
        /*/
            return 0.5f * (2f * p1 + t * (p2 - p0 + t * (2f * p0 - 5f * p1 + 4f * p2 - p3 + t * (3f * (p1 - p2) + p3 - p0))));
            //*/
        }


        public static Vector3 Get(ref Vector3 p0, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, float t)
        {
            Vector3 res =  0.5f * (2f * p1 + t * (p2 - p0 + t * (2f * p0 - 5f * p1 + 4f * p2 - p3 + t * (3f * (p1 - p2) + p3 - p0))));
            return res;
        }

        public static Vector3 GetFirstDerivative(ref Vector3 P0, ref Vector3 P1, ref Vector3 P2, ref Vector3 P3, float t)
        {
            Vector3 res = 0.5f*((-P0 + P2) +
                                2*(2*P0 - 5*P1 + 4*P2 - P3)*t +
                                3*(-P0 + 3*P1 - 3*P2 + P3)*t*t);
            return res;
        }

        public static Vector3 GetSecondDerivative(ref Vector3 P0, ref Vector3 P1, ref Vector3 P2, ref Vector3 P3, float t)
        {
            return 0.5f*(2*(2*P0 - 5*P1 + 4*P2 - P3) +
                         6*(-P0 + 3*P1 - 3*P2 + P3)*t);
        }

        public static float WorldDistanceToSplineDistance(Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3, float t, float worldDistance)
        {
            Vector3 derivative = GetFirstDerivative(ref P0, ref P1, ref P2, ref P3, t);
            float dt = derivative.magnitude;
            if (Mathf.Abs(dt) < float.Epsilon)
            {
                dt = float.Epsilon * Mathf.Sign(dt);
            }
            return worldDistance / dt;
        }

        static private float sqr (float v)
        {
            return v*v;
        }

        static public float GetCurvature(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 p = GetFirstDerivative(ref p0, ref p1, ref p2, ref p3, t); // prime
            Vector3 s = GetSecondDerivative(ref p0, ref p1, ref p2, ref p3, t); // second
            float d = p.magnitude;
            if (d < Mathf.Epsilon)
            {
                return float.MaxValue;
            }

            d = d*d*d;

            float n = Mathf.Sqrt(sqr(s.z*p.y - s.y*p.z) + sqr(s.x*p.z - s.z*p.x) + sqr(s.y*p.x - s.x*p.y));
            return n/d;
        }
    }
}
