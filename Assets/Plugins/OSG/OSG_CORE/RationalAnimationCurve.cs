// Old Skull Games
// Pierre Planeau
// Monday, February 18, 2019

namespace OSG.Core
{
    /// <summary>
    /// Rational (.Net Core) representation of a UnityEngine AnimationCurve.
    /// </summary>
    [System.Serializable]
    public class RationalAnimationCurve
    {
        [System.Serializable]
        public struct Keyframe
        {
            public Rational time; // x
            public Rational value; // y
            public Rational inTangent;
            public Rational outTangent;
        }

        public Keyframe[] keys;


        public RationalAnimationCurve(Keyframe[] keys)
        {
            if (keys == null || keys.Length < 2)
            {
                throw new System.ArgumentException("RationalAnimationCurve requires at least 2 Keyframes to be created.");
            }

            this.keys = keys;
        }

        /// <summary>
        /// Returns the first keyframe of the curve.
        /// </summary>
        /// <returns></returns>
        public Keyframe GetFirstKey()
        {
            return keys[0];
        }

        /// <summary>
        /// Returns the last keyframe of the curve.
        /// </summary>
        /// <returns></returns>
        public Keyframe GetLastKey()
        {
            return keys[keys.Length - 1];
        }

        /// <summary>
        /// Evaluates the curve at time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Rational Evaluate(Rational time)
        {
            Keyframe keyframe0 = keys[0];
            Keyframe keyframe1 = keys[1];

            for (int i = keys.Length - 1; --i > 0;)
            {
                if (keys[i].time < time)
                {
                    keyframe0 = keys[i];
                    keyframe1 = keys[i + 1];
                    break;
                }
            }

            Rational t = Rational.InverseLerp(keyframe0.time, keyframe1.time, time);

            return Evaluate(t, keyframe0, keyframe1);
        }

        /// <summary>
        /// Evaluates the curve described by two keyframes using an interpolant value between [0, 1].
        /// </summary>
        /// <param name="t">Interpolant value between [0, 1].</param>
        /// <param name="keyframe0"></param>
        /// <param name="keyframe1"></param>
        /// <returns></returns>
        public static Rational Evaluate(Rational t, Keyframe keyframe0, Keyframe keyframe1)
        {
            Rational dt = keyframe1.time - keyframe0.time;

            Rational m0 = keyframe0.outTangent * dt;
            Rational m1 = keyframe1.inTangent * dt;

            Rational tSquared = t * t;
            Rational tTimes2 = t * 2;
            Rational oneMinusT = 1 - t;
            Rational oneMinusTSquared = oneMinusT * oneMinusT;

            Rational a = (1 + tTimes2) * oneMinusTSquared;
            Rational b = t * oneMinusTSquared;
            Rational c = tSquared * (3 - tTimes2);
            Rational d = tSquared * (t - 1);

            return a * keyframe0.value + b * m0 + c * keyframe1.value + d * m1;
        }
    }


    [System.Serializable]
    public class IntegerAnimationCurve
    {
        [System.Serializable]
        public struct Keyframe
        {
            public int time; // x
            public int value; // y
            public int inTangent;
            public int outTangent;


            public Keyframe(int time, int value, int inTangent, int outTangent)
            {
                this.time = time;
                this.value = value;
                this.inTangent = inTangent;
                this.outTangent = outTangent;
            }
        }

        public Keyframe[] keys;


        public IntegerAnimationCurve(Keyframe[] keys)
        {
            if (keys == null || keys.Length < 2)
            {
                throw new System.ArgumentException("IntegerAnimationCurve requires at least 2 Keyframes to be created.");
            }

            this.keys = keys;
        }

        /// <summary>
        /// Returns the first keyframe of the curve.
        /// </summary>
        /// <returns></returns>
        public Keyframe GetFirstKey()
        {
            return keys[0];
        }

        /// <summary>
        /// Returns the last keyframe of the curve.
        /// </summary>
        /// <returns></returns>
        public Keyframe GetLastKey()
        {
            return keys[keys.Length - 1];
        }

        /// <summary>
        /// Evaluates the curve at time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int Evaluate(int time)
        {
            Keyframe keyframe0 = keys[0];
            Keyframe keyframe1 = keys[1];

            for (int i = keys.Length - 1; --i > 0;)
            {
                if (keys[i].time < time)
                {
                    keyframe0 = keys[i];
                    keyframe1 = keys[i + 1];
                    break;
                }
            }

            //int t = Rational.InverseLerp(keyframe0.time, keyframe1.time, time);
            int t = CoreMath.IntInverseLerp(keyframe0.time, keyframe1.time, time).ToMilli();

            return Evaluate(t, keyframe0, keyframe1);
        }

        /// <summary>
        /// Evaluates the curve described by two keyframes using an interpolant value between [0, 1000].
        /// </summary>
        /// <param name="t">Interpolant value between [0, 1000].</param>
        /// <param name="keyframe0"></param>
        /// <param name="keyframe1"></param>
        /// <returns></returns>
        public static int Evaluate(int t, Keyframe keyframe0, Keyframe keyframe1)
        {
            long dt = keyframe1.time - keyframe0.time;

            long m0 = (keyframe0.outTangent * dt) / 1000;
            long m1 = (keyframe1.inTangent * dt) / 1000;

            long tSquared = (t * t) / 1000;
            long tTimes2 = t * 2;
            long oneMinusT = 1000 - t;
            long oneMinusTSquared = (oneMinusT * oneMinusT) / 1000;

            long a = ((1000 + tTimes2) * oneMinusTSquared) / 1000;
            long b = (t * oneMinusTSquared) / 1000;
            long c = (tSquared * (3000 - tTimes2)) / 1000;
            long d = (tSquared * (t - 1000)) / 1000;

            long result = ((a * keyframe0.value) + (b * m0) + (c * keyframe1.value) + (d * m1)) / 1000;

            if (result > int.MaxValue)
                return int.MaxValue;
            else if (result < int.MinValue)
                return int.MinValue;
            return (int)result;
        }
    }
}
