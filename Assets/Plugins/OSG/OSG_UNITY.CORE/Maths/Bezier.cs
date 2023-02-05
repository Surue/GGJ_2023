// Old Skull Games
// Benoit Constantin
// Monday, June 17, 2019

//Source : https://fr.wikipedia.org/wiki/Courbe_de_B%C3%A9zier


using UnityEngine;

namespace OSG
{
    public static class Bezier
    {
        /// <summary>
        /// Bezier quadratic, not continuous in P0 and P2
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="controlPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="t">between 0 and 1</param>
        /// <returns></returns>
        public static Vector2 Bezier2(Vector2 firstPoint, Vector2 controlPoint, Vector2 secondPoint, float t)
        {
            float a = (1f - t);
            float a2 = a * a;

            return firstPoint * a2 + 2 * controlPoint * t * a + secondPoint * t * t;
        }

        /// <summary>
        /// Bezier cubic, continuous in P0 and P3, but more costly than Bezier2
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="firstControl"></param>
        /// <param name="secondControl"></param>
        /// <param name="secondPoint"></param>
        /// <param name="t">between 0 and 1</param>
        /// <returns></returns>
        public static Vector2 Bezier3(Vector2 firstPoint, Vector2 firstControl, Vector2 secondControl, Vector2 secondPoint, float t)
        {
            float a = (1f - t);
            float a2 = a * a;
            float t2 = t * t;

            return firstPoint * a2 * a + 3 * firstControl * t * a2 + 3 * secondControl * t2 * a + secondPoint * t2 * t;
        }

        public static Vector2 NearestPointOnBezier3()
        {
            return Vector2.zero; // TO DO BENOIT
        }

        /// <summary>
        /// Bezier quadratic, not continuous in P0 and P2
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="controlPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="t">between 0 and 1</param>
        /// <returns></returns>
        public static Vector3 Bezier2(Vector3 firstPoint, Vector3 controlPoint, Vector3 secondPoint, float t)
        {
            float a = (1f - t);
            float a2 = a * a;

            return firstPoint * a2 + 2 * controlPoint * t * a + secondPoint * t * t;
        }


        /// <summary>
        /// Bezier cubic, continuous in P0 and P3, but more costly than Bezier2
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="firstControl"></param>
        /// <param name="secondControl"></param>
        /// <param name="secondPoint"></param>
        /// <param name="t">between 0 and 1</param>
        /// <returns></returns>
        public static Vector3 Bezier3(Vector3 firstPoint, Vector3 firstControl, Vector3 secondControl, Vector3 secondPoint, float t)
        {
            float a = (1f - t);
            float a2 = a * a;
            float t2 = t * t;

            return firstPoint * a2 * a + 3 * firstControl * t * a2 + 3 * secondControl * t2 * a + secondPoint * t2 * t;
        }
    }
}