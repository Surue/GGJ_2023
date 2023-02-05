// Old Skull Games
// Robin Blanc
// Thursday, February 14, 2019

using OSG.Core;
using UnityEngine;

namespace OSG
{
    public static class AnimationCurveExtensions
    {
        public static float EvaluateInverse(this AnimationCurve curve, float value, int accuracy=15)
        {
            if(curve.keys.Length==0)
            {
                Debug.LogError("Null curve given");
                return value;
            }

            float startTime = curve.keys [0].time;
            float endTime = curve.keys [curve.length - 1].time;
            float nearestTime = startTime;
            float step = endTime - startTime;

            for (int i = 0; i < accuracy; i++) {

                float valueAtNearestTime = curve.Evaluate (nearestTime);
                float distanceToValueAtNearestTime = Mathf.Abs (value - valueAtNearestTime);

                float timeToCompare = nearestTime + step;
                float valueAtTimeToCompare = curve.Evaluate (timeToCompare);
                float distanceToValueAtTimeToCompare = Mathf.Abs (value - valueAtTimeToCompare);

                if (distanceToValueAtTimeToCompare < distanceToValueAtNearestTime) {
                    nearestTime = timeToCompare;
                    valueAtNearestTime = valueAtTimeToCompare;
                }
                step = Mathf.Abs(step * 0.5f) * Mathf.Sign(value-valueAtNearestTime);
            }

            return nearestTime;
        }

        /// <summary>
        /// Returns the last keyframe of the curve.
        /// </summary>
        public static Keyframe GetLastKey(this AnimationCurve curve)
        {
            return curve.keys[curve.length - 1];
        }

        /// <summary>
        /// Returns the time of the last keyframe.
        /// </summary>
        public static float GetDuration(this AnimationCurve curve)
        {
            return curve.GetLastKey().time;
        }

        /// <summary>
        /// Converts a UnityEngine.AnimationCurve to a RationalAnimationCurve.
        /// </summary>
        /// <param name="animationCurve"></param>
        /// <returns></returns>
        public static RationalAnimationCurve ToRationalAnimationCurve(this AnimationCurve animationCurve)
        {
            var animationCurveKeys = animationCurve.keys;
            var rationalKeyframes = new RationalAnimationCurve.Keyframe[animationCurveKeys.Length];

            for (int i = animationCurveKeys.Length; --i >= 0;)
            {
                rationalKeyframes[i] = new RationalAnimationCurve.Keyframe()
                {
                    time = Rational.FromDouble(animationCurveKeys[i].time),
                    value = Rational.FromDouble(animationCurveKeys[i].value),
                    inTangent = Rational.FromDouble(animationCurveKeys[i].inTangent),
                    outTangent = Rational.FromDouble(animationCurveKeys[i].outTangent)
                };
            }

            return new RationalAnimationCurve(rationalKeyframes);
        }

        /// <summary>
        /// Converts a UnityEngine.AnimationCurve to a IntegerAnimationCurve.
        /// </summary>
        /// <param name="animationCurve"></param>
        /// <returns></returns>
        public static IntegerAnimationCurve ToIntegerAnimationCurve(this AnimationCurve animationCurve)
        {
            var animationCurveKeys = animationCurve.keys;
            var rationalKeyframes = new IntegerAnimationCurve.Keyframe[animationCurveKeys.Length];

            for (int i = animationCurveKeys.Length; --i >= 0;)
            {
                rationalKeyframes[i] = new IntegerAnimationCurve.Keyframe()
                {
                    time = Mathf.RoundToInt(animationCurveKeys[i].time),
                    value = Mathf.RoundToInt(animationCurveKeys[i].value),
                    inTangent = Mathf.RoundToInt(animationCurveKeys[i].inTangent * 1000f),
                    outTangent = Mathf.RoundToInt(animationCurveKeys[i].outTangent * 1000f)
                };
            }

            return new IntegerAnimationCurve(rationalKeyframes);
        }

        /// <summary>
        /// Gets value at time t regardless of the curve's wrapping mode.
        /// </summary>
        public static float EvaluateOutsideBounds(this AnimationCurve curve, float t)
        {
            // Curve has no key
            if (curve.keys.Length == 0)
            {
                return 0f;
            }
            else if (curve.keys.Length == 1) // Curve has one single key
            {
                return curve.keys[0].value;
            }

            // Requested time is within the curve's range
            Keyframe firstKey = curve.keys[0];
            Keyframe lastKey = curve.GetLastKey();

            if (t >= firstKey.time && t <= lastKey.time)
            {
                return curve.Evaluate(t);
            }

            // Actual function to get the t evaluation ouf of the curve range
            float value = 0f;
            if (t < firstKey.time)
            {
                float timeOffset = t - firstKey.time;
                value = firstKey.value + firstKey.outTangent * timeOffset;
            }
            else
            {
                float timeOffset = t - lastKey.time;
                value = lastKey.value + lastKey.inTangent * timeOffset;
            }

            return value;
        }
    }
}