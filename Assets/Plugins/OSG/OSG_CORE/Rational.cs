// Old Skull Games
// Pierre Planeau
// Friday, January 19, 2018

// Original: https://gist.github.com/maraigue/739403

using System;

namespace OSG.Core
{
    [Serializable]
    public struct Rational : IComparable
    {
        public int _numerator/* = 0*/; // We need it to be public so that the Unity Serializer (network) can find it.
        /*public int numerator
        {
            get { return _numerator; }
            set { _numerator = value; }
        }*/

        public int _denominator/* = 1*/;
        /*public int denominator
        {
            get { return _denominator; }
            set
            {
                if (0 == value)
                {
                    throw new ArithmeticException("Denominator must not be 0.");
                }
                _denominator = value;
            }
        }*/


        #region Initialization
        
        /*public Rational()
        {
            _numerator = 0;
            _denominator = 1;
            Set(0, 1);
        }*/

        public Rational(int int_value)
        {
            _numerator = int_value;
            _denominator = 1;
            //Set(int_value, 1);
        }

        public Rational(int new_numerator, int new_denominator)
        {
            _numerator = 0;
            _denominator = 1;

            Set(new_numerator, new_denominator);
        }

        public Rational(Rational r)
        {
            _numerator = 0;
            _denominator = 1;

            Set(r._numerator, r._denominator);
        }


        public void Set(int new_numerator, int new_denominator)
        {
            if (0 == new_denominator)
            {
                throw new ArithmeticException("Denominator must not be 0.");
            }
            else if (0 == new_numerator)
            {
                _numerator = 0;
                _denominator = 1;
                return;
            }
            else if (1 == new_denominator)
            {
                _numerator = new_numerator;
                _denominator = new_denominator;
                return;
            }

            _numerator = new_numerator;
            _denominator = new_denominator;

            _regularize();
        }

        public void Approximate()
        {
            while ((_denominator > 10000) && (_numerator != 0))
            {
                _denominator /= 10;
                _numerator /= 10;
            }
        }

        #endregion

        #region Shortcuts

        /// <summary>
        /// Shorthand for writing Rational(0, 1)
        /// </summary>
        public static Rational zero { get { return new Rational() { _numerator = 0, _denominator = 1 }; } }
        /// <summary>
        /// Shorthand for writing Rational(1, 1)
        /// </summary>
        public static Rational one { get { return new Rational() { _numerator = 1, _denominator = 1 }; } }
        /// <summary>
        /// Shorthand for writing Rational(1, 2)
        /// </summary>
        public static Rational half { get { return new Rational() { _numerator = 1, _denominator = 2 }; } }
        /// <summary>
        /// Shorthand for writing Rational(1, 3)
        /// </summary>
        public static Rational third { get { return new Rational() { _numerator = 1, _denominator = 3 }; } }

        #endregion

        #region Utility

        public int GetDenominator()
        {
            return _denominator;
        }

        public int GetNumerator()
        {
            return _numerator;
        }

        public int Floor()
        {
            _regularize();
            return _numerator / _denominator;
        }

        public int Round()
        {
            int floor = Floor();

            if ((this - floor) >= half)
                return floor + 1;

            return floor;
        }

        /// <summary>
        /// Returns the result of (numerator * 10) / denominator .
        /// </summary>
        /// <returns></returns>
        public int ToDeci()
        {
            _regularize();
            return (_numerator * 10) / _denominator;
        }

        /// <summary>
        /// Returns the result of (numerator * 100) / denominator .
        /// </summary>
        /// <returns></returns>
        public int ToCenti()
        {
            _regularize();
            return (_numerator * 100) / _denominator;
        }

        /// <summary>
        /// Returns the result of (numerator * 1000) / denominator .
        /// </summary>
        /// <returns></returns>
        public int ToMilli()
        {
            _regularize();
            return (_numerator * 1000) / _denominator;
        }

        /// <summary>
        /// Returns true when the Rational value is zero.
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return _numerator == 0;
        }

        /// <summary>
        /// Returns true when the numerator and denominator are equal.
        /// </summary>
        /// <returns></returns>
        public bool IsOne()
        {
            return _numerator == _denominator;
        }

        /// <summary>
        /// Elevate Rational to a power.
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public Rational Power(int power)
        {
            if (power <= 0)
                return one;
            else if (power == 1)
                return new Rational(this);

            Rational result = new Rational(this);
            while (power-- > 1)
            {
                result *= this;
            }

            return result;
        }

        #endregion

        #region Static Functions

        /// <summary>
        /// Returns the Greatest Common Divisor between the given integers.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static int Gcd(int v1, int v2)
        {
            int tmp;

            if (0 == v1 || 0 == v2)
                return 0;

            if (v1 < 0) v1 = -v1;
            if (v2 < 0) v2 = -v2;

            if (v2 > v1)
            {
                tmp = v1; v1 = v2; v2 = tmp;
            }

            for (; ; )
            {
                tmp = v1 % v2;
                if (0 == tmp)
                    return v2;

                v1 = v2; v2 = tmp;
            }
        }

        /// <summary>
        /// Least Common Multiple.
        /// ex: a = 8, b = 12, LCM = 24
        /// because 8 * 3 = 24 and 12 * 2 = 24
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int LCM(int a, int b)
        {
            int old_a = a;
            int old_b = b;

            if (a > b)
            {
                int tmp = a;
                a = b;
                b = tmp;
            }

            while (a <= b)
            {
                if (a % b == 0)
                    return a;
                a += a;
            }
            return old_a * old_b;
        }

        /// <summary>
        /// Generates a random Rational between 0/precision and precision/precision (between 0 and 1).
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static Rational RandomRational01(int precision = 1000)
        {
            return RandomRational01((min, max) => { return new System.Random().Next(min, max + 1); }, precision);
        }

        /// <summary>
        /// Generates a random Rational between 0/precision and precision/precision (between 0 and 1), using a custom random function.
        /// </summary>
        /// <param name="MinMaxInclusiveRandFunction">Custom random integer function returning an integer between [min, max].</param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static Rational RandomRational01(Func<int, int, int> MinMaxInclusiveRandFunction, int precision = 1000)
        {
            return new Rational(MinMaxInclusiveRandFunction(0, precision), precision);
        }

        public static int Floor(Rational r)
        {
            return r.Floor();
        }

        /// <summary>
        /// Returns the absolute of the given Rational.
        /// ex: (-1/2) will give (1/2)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Rational Absolute(Rational r)
        {
            int newNumerator = r._numerator;
            int newDenominator = r._denominator;

            if (newNumerator < 0)
                newNumerator = -newNumerator;
            if (newDenominator < 0)
                newDenominator = -newDenominator;

            return new Rational(newNumerator, newDenominator);
        }

        /// <summary>
        /// Calculates the linear parameter t that produces the interpolant value within the range [a, b].
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Rational InverseLerp(Rational a, Rational b, Rational value)
        {
            if (value >= b)
                return Rational.one;
            else if (value <= a)
                return Rational.zero;

            return (value - a) / (b - a);
        }

        /// <summary>
        /// Creates a Rational from a double.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="MaximumDenominator">Default value is good for doubles inferior to 2 but could be set to 65536 for doubles that are equal or greater than 2.</param>
        /// <returns></returns>
        public static Rational FromDouble(double f, long MaximumDenominator = 4096)
        {
            // original: https://rosettacode.org/wiki/Convert_decimal_number_to_rational#C.23
            //  a: continued fraction coefficients.

            var h = new long[3] { 0, 1, 0 };
            var k = new long[3] { 1, 0, 0 };
            long n = 1;
            bool neg = false;

            if (MaximumDenominator <= 1)
            {
                return new Rational((int) f, 1);
            }

            if (f < 0)
            {
                neg = true;
                f = -f;
            }

            while (f != Math.Floor(f))
            {
                n <<= 1;
                f *= 2;
            }
            long d = (long)f;

            // continued fraction and check denominator each step
            for (int i = 0; i < 64; i++)
            {
                long a = (n != 0) ? d / n : 0;

                if ((i != 0) && (a == 0))
                    break;

                long x = d;
                d = n;
                n = x % n;

                x = a;
                if (k[1] * a + k[0] >= MaximumDenominator)
                {
                    x = (MaximumDenominator - k[0]) / k[1];
                    if (x * 2 >= a || k[1] >= MaximumDenominator)
                        i = 65;
                    else
                        break;
                }

                h[2] = x * h[1] + h[0]; h[0] = h[1]; h[1] = h[2];
                k[2] = x * k[1] + k[0]; k[0] = k[1]; k[1] = k[2];
            }

            var Denominator = k[1];
            var Numerator = neg ? -h[1] : h[1];

            return new Rational((int)Numerator, (int)Denominator);
        }

        #endregion

        #region Internal Functions

        private void _fix_denominator(ref Rational other)
        {
            if (0 == other._denominator)
            {
                throw new ArithmeticException("Denominator must not be 0.");
            }
            else if (_denominator == other._denominator)
            {
                return;
            }

            int lcm = LCM(_denominator, other._denominator);

            int mulDenominator = lcm / _denominator;
            int mulOtherDenominator = lcm / other._denominator;

            _numerator = _numerator * mulDenominator;
            _denominator = lcm;

            other._numerator = other._numerator * mulOtherDenominator;
            other._denominator = lcm;

            /*int tmp = _denominator;
            _numerator *= other._denominator;
            _denominator *= other._denominator;

            other._numerator *= tmp;
            other._denominator *= tmp;*/
        }

        private void _regularize()
        {
            Approximate();

            int divisor = CoreMath.Sign(_denominator) * Gcd(_numerator, _denominator);
            if (divisor == 0)
            {
                _numerator = 0;
                _denominator = 1;
            }
            else
            {
                _numerator /= divisor;
                _denominator /= divisor;
            }
        }

        #endregion

        #region Operators

        public static bool operator ==(Rational r1, Rational r2)
        {
            r1._regularize();
            r2._regularize();
            return (r1._numerator == r2._numerator && r1._denominator == r2._denominator);
        }

        public static bool operator !=(Rational r1, Rational r2)
        {
            r1._regularize();
            r2._regularize();
            return (r1._numerator != r2._numerator || r1._denominator != r2._denominator);
        }

        public static bool operator <(Rational r1, Rational r2)
        {
            Rational tmp1 = new Rational(r1);
            Rational tmp2 = new Rational(r2);

            tmp1._fix_denominator(ref tmp2);
            return (tmp1._numerator < tmp2._numerator);
        }

        public static bool operator >(Rational r1, Rational r2)
        {
            return (r2 < r1);
        }

        public static bool operator <=(Rational r1, Rational r2)
        {
            Rational tmp1 = new Rational(r1);
            Rational tmp2 = new Rational(r2);

            tmp1._fix_denominator(ref tmp2);
            return (tmp1._numerator <= tmp2._numerator);
        }

        public static bool operator >=(Rational r1, Rational r2)
        {
            return (r2 <= r1);
        }

        public static bool operator <(Rational r, int i)
        {
            return r < (Rational)i;
        }

        public static bool operator >(Rational r, int i)
        {
            return (Rational)i < r;
        }

        public static bool operator <=(Rational r, int i)
        {
            return r <= (Rational)i;
        }

        public static bool operator >=(Rational r, int i)
        {
            return (Rational)i <= r;
        }

        public static bool operator <(int i, Rational r)
        {
            return (Rational)i < r;
        }

        public static bool operator >(int i, Rational r)
        {
            return r < (Rational)i;
        }

        public static bool operator <=(int i, Rational r)
        {
            return (Rational)i <= r;
        }

        public static bool operator >=(int i, Rational r)
        {
            return r <= (Rational)i;
        }

        public static explicit operator double(Rational r)
        {
            return (double)r._numerator / (double)r._denominator;
        }

        public static explicit operator float(Rational r)
        {
            return (float)r._numerator / (float)r._denominator;
        }

        public static implicit operator Rational(int i)
        {
            return new Rational(i);
        }

        public static Rational operator +(Rational r)
        {
            return new Rational(r._numerator, r._denominator);
        }

        public static Rational operator -(Rational r)
        {
            return new Rational(-r._numerator, r._denominator);
        }

        public static Rational operator +(Rational r1, Rational r2)
        {
            Rational tmp1 = new Rational(r1);
            Rational tmp2 = new Rational(r2);

            tmp1._fix_denominator(ref tmp2);
            return new Rational(tmp1._numerator + tmp2._numerator, tmp1._denominator);
        }

        public static Rational operator -(Rational r1, Rational r2)
        {
            Rational tmp1 = new Rational(r1);
            Rational tmp2 = new Rational(r2);

            tmp1._fix_denominator(ref tmp2);
            return new Rational(tmp1._numerator - tmp2._numerator, tmp1._denominator);
        }

        public static Rational operator *(Rational r1, Rational r2)
        {
            return new Rational(r1._numerator * r2._numerator, r1._denominator * r2._denominator);
        }

        public static Rational operator /(Rational r1, Rational r2)
        {
            if (r2._numerator == 0)
            {
                throw new DivideByZeroException();
            }
            return new Rational(r1._numerator * r2._denominator, r1._denominator * r2._numerator);
        }

        #endregion

        #region Generic

        public override string ToString()
        {
            _regularize();
            if (_denominator == 1)
                return _numerator.ToString();

            float f = (float) this;
            return string.Format("{0}/{1} ({2})", _numerator, _denominator, f.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public string ToStringWithDigits(int digits = 2)
        {
            _regularize();
            if (_denominator == 1)
                return _numerator.ToString();

            if (digits < 1)
                digits = 1;

            string strDigits = "";

            for (int i = 0; i < digits; ++i)
            {
                strDigits += "0";
            }

            return ((float)this).ToString("0." + strDigits);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                return (this == (Rational)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            throw new System.NotSupportedException("Rational.GetHashCode() is unsupported.");
            //return (_numerator | (_denominator << 16));
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            if (!(obj is Rational))
            {
                return 1;
            }

            Rational r = (Rational) obj;

            if (this > r)
                return 1;  // this is greater
            else if (this < r)
                return -1; // this is lower
            else
                return 0;  // equal to each other
        }

        #endregion
    }
}
