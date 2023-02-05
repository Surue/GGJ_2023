// Old Skull Games
// Pierre Planeau
// Friday, May 18, 2018


namespace OSG.Core
{
    public static class CoreMath
    {
        /// <summary>
        /// Integer Squared Root.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int IntSqrt(int num)
        {
            if (0 == num) { return 0; }  // Avoid zero divide  
            int n = (num / 2) + 1;       // Initial estimate, never low  
            int n1 = (n + (num / n)) / 2;
            while (n1 < n)
            {
                n = n1;
                n1 = (n + (num / n)) / 2;
            }
            return n;
        }

        /// <summary>
        /// Long Integer Squared Root.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static long LongSqrt(long num)
        {
            if (0L == num) { return 0L; }  // Avoid zero divide  
            long n = (num / 2L) + 1L;       // Initial estimate, never low  
            long n1 = (n + (num / n)) / 2L;
            while (n1 < n)
            {
                n = n1;
                n1 = (n + (num / n)) / 2L;
            }
            return n;
        }

        public static int Max(int a, int b)
        {
            return (a >= b) ? a : b;
        }

        public static int Min(int a, int b)
        {
            return (a <= b) ? a : b;
        }

        public static T Clamp<T>(T x, T min, T max) where T : System.IComparable
        {
            var _Result = x;
            if (x.CompareTo(max) > 0)
                _Result = max;
            else if (x.CompareTo(min) < 0)
                _Result = min;
            return _Result;
        }

        public static int Abs(int i)
        {
            return Max(i, -i);
        }

        /// <summary>
        /// Returns an integer that indicates the sign of a 32-bit signed integer.
        /// </summary>
        /// <param name="i">A signed number</param>
        /// <returns>-1 when i is less than 0, 0 when i is equal to 0, 1 when i is greater than 0</returns>
        public static int Sign(int i)
        {
            /*if (i > 0)
                return 1;
            if (i < 0)
                return -1;
            return 0;*/
            return (i > 0) ? 1 : ((i < 0) ? -1 : 0);
        }

        /// <summary>
        /// Returns an integer that indicates the sign of a Rational.
        /// </summary>
        /// <param name="r"></param>
        /// <returns>-1 when r is less than 0, 0 when r is equal to 0, 1 when r is greater than 0</returns>
        public static int Sign(Rational r)
        {
            /*if (i > 0)
                return 1;
            if (i < 0)
                return -1;
            return 0;*/
            return (r > Rational.zero) ? 1 : ((r < Rational.zero) ? -1 : 0);
        }

        /// <summary>
        /// Returns a string that indicates the sign of a 32-bit signed integer.
        /// </summary>
        /// <param name="i">A signed number</param>
        /// <returns>"-" when i is less than 0, "" when i is equal to 0, "+" when i is greater than 0</returns>
        public static string SignString(int i)
        {
            return (i > 0) ? "+" : ((i < 0) ? "-" : "");
        }

        public static float Lerp(float a, float b, float t)
        {
            t = Clamp(t, 0f, 1f);
            return (a * (1f - t) + (b * t));
        }

        public static float InverseLerp(float from, float to, float value)
        {
            if (from < to)
            {
                if (value < from)
                    return 0.0f;
                else if (value > to)
                    return 1.0f;
            }
            else
            {
                if (value < to)
                    return 1.0f;
                else if (value > from)
                    return 0.0f;
            }
            return (value - from) / (to - from);
        }

        public static double InverseLerp(double from, double to, double value)
        {
            if (from < to)
            {
                if (value < from)
                    return 0.0;
                else if (value > to)
                    return 1.0;
            }
            else
            {
                if (value < to)
                    return 1.0;
                else if (value > from)
                    return 0.0;
            }
            return (value - from) / (to - from);
        }

        public static int Squared(this int i)
        {
            return i * i;
        }

        public static int[] LongToInts(long a)
        {
            int a1 = (int)(a & uint.MaxValue);
            int a2 = (int)(a >> 32);
            return new int[] { a1, a2 };
        }

        public static long IntsToLong(int a1, int a2)
        {
            long b = a2;
            b = b << 32;
            b = b | (uint)a1;
            return b;
        }


        /// <summary>
        /// Linearly interpolates between two integers.
        /// 't' ranges from 0 to 1000.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int IntLerp(int a, int b, int t)
        {
            t = Clamp(t, 0, 1000);
            return (a * (1000 - t) + (b * t)) / 1000;
        }

        /// <summary>
        /// Calculates the linear parameter t that produces the interpolant value within the range [min, max].
        /// Works with any min, max and val.
        /// t is retunred as a Rational.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Rational IntInverseLerp(int min, int max, int val)
        {
            val = Clamp(val, min, max);
            return new Rational((val - min), (max - min));
        }

        /// <summary>
        /// Rotates the given (x1, y1) by 45 degrees counter clockwise (following the trigo circle).
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static void Rotate45(int x1, int y1, out Rational x2, out Rational y2)
        {
            // theta = PI/4
            // x2 = x1 * cos(theta) - y1 * sin(theta)
            // y2 = x1 * sin(theta) + y1 * cos(theta)

            var cosTheta = new Rational(7071, 10000);
            var sinTheta = cosTheta;

            x2 = x1 * cosTheta - y1 * sinTheta;
            y2 = x1 * sinTheta + y1 * cosTheta;
        }
    }
}
