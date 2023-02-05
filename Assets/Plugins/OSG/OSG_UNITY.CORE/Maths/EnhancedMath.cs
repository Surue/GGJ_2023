using UnityEngine;

namespace OSG
{
    public static class EnhancedMath
    {
        public const float inch2cm = 2.54f;
        public const float unitPerSec2KmPerHour = 3.6f;
        public const float kmPerHour2UnitPerSec = 1f / 3.6f;

        // Ease in AnimationCurve
        public static AnimationCurve easeIn
        {
            get
            {
                return new AnimationCurve(
                    new Keyframe(0, 0, 0, 0, 0, 0.25f),
                    new Keyframe(1, 1, 0, 0, 1, 0));
            }
        }
        public static AnimationCurve easeOut
        {
            get
            {
                return new AnimationCurve(
                    new Keyframe(0, 1, 0, 0, 0, 0.25f),
                    new Keyframe(1, 0, 0, 0, 1, 0));
            }
        }

        /// <summary>
        /// Simplified Pow function for ints.
        /// </summary>
        public static int IntPow(int value, int power)
        {
            if (power < 1)
                return 1;

            int baseValue = value;
            for (int i = 1; i < power; i++)
            {
                value *= baseValue;
            }

            return value;
        }

        /// <summary>
        /// Clamps an int value between two int limits.
        /// </summary>
        public static int IntClamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            return value;
        }


        /// <summary>
        /// Deltatime-independant float clamp.
        /// </summary>
        public static float Damp(float a, float b, float lambda, float dt)
        {
            return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        /// <summary>
        /// Deltatime-independant Vector3 clamp.
        /// </summary>
        public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt)
        {
            return Vector3.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        /// <summary>
        /// Deltatime-independant Quaternion clamp.
        /// </summary>
        public static Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }


        /// <summary>
        /// Source: https://github.com/setchi/Unity-LineSegmentsIntersection
        /// By setchi
        /// </summary>
        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }

            intersection.x = p1.x + u * (p2.x - p1.x);
            intersection.y = p1.y + u * (p2.y - p1.y);

            return true;
        }

        /// <summary>
        /// Short version for the above function (no out parameter)
        /// </summary>
        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4)
        {
            var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

            if (d == 0.0f)
            {
                return false;
            }

            var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
            var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Retrieves the size of the frustum of a camera at a given distance.
        /// </summary>
        public static Vector2 GetCameraFrustumAtDistance(Camera camera, float distance)
        {
            float frustumHeight = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * camera.aspect, frustumHeight);
        }

        /// <summary>
        /// Retrieves the size of a frustum with given FOV, aspect ratio and distance.
        /// </summary>
        public static Vector2 GetFrustumAtDistance(float FOV, float aspect, float distance)
        {
            float frustumHeight = 2.0f * distance * Mathf.Tan(FOV * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * aspect, frustumHeight);
        }

        /// <summary>
        /// Retrieves the size of the frustum at given distance and FOV for the current screen ratio.
        /// </summary>
        public static Vector2 GetScreenFrustumAtDistance(float FOV, float distance)
        {
            float frustumHeight = 2.0f * distance * Mathf.Tan(FOV * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * Screen.currentResolution.width / Screen.currentResolution.height, frustumHeight);
        }

        /// <summary>
        /// Retrieves the distance of a camera for a given frustum height.
        /// </summary>
        public static float GetDistanceFromFrustumHeight(float FOV, float frustumHeight)
        {
            return frustumHeight * 0.5f / Mathf.Tan(FOV * 0.5f * Mathf.Deg2Rad);
        }


        /// <summary>
        /// Retrieves the four points of a frustum rect drawn with the given FOV, aspect ratio and distance.
        /// </summary>
        public static Vector3[] GetFrustumCorners(Vector3 position, Quaternion rotation, float FOV, float aspect, float distance)
        {
            return GetRectCorners(position, rotation, GetFrustumAtDistance(FOV, aspect, distance));
        }

        /// <summary>
        /// Retrieves the four points of a frustum rect drawn for the given camera and distance.
        /// </summary>
        public static Vector3[] GetFrustumCorners(Vector3 position, Quaternion rotation, Camera camera, float distance)
        {
            return GetRectCorners(position, rotation, GetCameraFrustumAtDistance(camera, distance));
        }

        /// <summary>
        /// Retrieves the four points of a rect (Bottom left, top left, top right and bottom right).
        /// </summary>
        public static Vector3[] GetRectCorners(Vector3 position, Quaternion rotation, Vector2 rectSize)
        {
            rectSize *= 0.5f;

            Vector3[] array = new Vector3[4]
            {
                position - rotation * rectSize,
                position + rotation * new Vector2(-rectSize.x, rectSize.y),
                position + rotation * rectSize,
                position + rotation * new Vector2(rectSize.x, -rectSize.y)
            };

            return array;
        }

        /// <summary>
        /// Retrieves the four points of a frustum rect drawn at a given distance from a specified Camera.
        /// </summary>
        public static Vector3[] GetFrustumCorners(Camera camera, float distance)
        {
            return GetRectCorners(camera.transform.position + camera.transform.forward * distance, camera.transform.rotation, GetCameraFrustumAtDistance(camera, distance));
        }


        /// <summary>
        /// 2D PerlinNoise with extra scale, magnitude and power parameters.
        /// </summary>
        public static float EnhancedPerlionNoise(float x, float y, float scale, float mag, float power)
        {
            // Can't divide by zero
            if (x == 0f || y == 0f)
                return 1;

            return Mathf.Pow(Mathf.PerlinNoise(x / scale, y / scale) * mag, power);
        }


        /// <summary>
        /// Retrieves the shortest distance of a point from a line
        /// </summary>
        public static float DistanceToLine(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }

        /// <summary>
        /// Retrieves the shortest distance of a point from a line
        /// </summary>
        public static float PointDistanceToLine(Vector3 origin, Vector3 direction, Vector3 point)
        {
            return Vector3.Cross(direction, point - origin).magnitude;
        }

        /// <summary>
        /// Retrieves the closest point on an infinite line.
        /// Source: https://stackoverflow.com/a/51906100
        /// </summary>
        public static Vector3 GetNearestPointOnInfiniteLine(Vector3 origin, Vector3 direction, Vector3 point)
        {
            direction.Normalize();
            Vector3 lhs = point - origin;

            float dotP = Vector3.Dot(lhs, direction);
            return origin + direction * dotP;
        }

        /// <summary>
        /// Retrieves the closest point on an finite line (out point is clamped to the line itself).
        /// Source: https://stackoverflow.com/a/51906100
        /// </summary>
        public static Vector3 GetNearestPointOnFiniteLine(Vector3 origin, Vector3 end, Vector3 point)
        {
            //Get heading
            Vector3 heading = end - origin;
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            //Do projection from the point but clamp it
            Vector3 lhs = point - origin;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + heading * dotP;
        }


        /// <summary>
        /// Lerps towards the target value with a wobbling effect.
        /// </summary>
        /// <param name="value">The value to lerp.</param>
        /// <param name="target">The value to lerps towards.</param>
        /// <param name="acceleration">The acceleration factor, clamped in-between 0 and 1. It should be time dependant.</param>
        /// <param name="velocity">Velocity must be passed as a ref as it is edited each time this function is called.</param>
        /// <param name="elasticity">A value clamped in-between 0 and 1 that drives the wobble effect near the target value. The higher it is, the bigger is the wobble.</param>
        /// <returns></returns>
        public static float ElasticLerp(float value, float target, float acceleration, ref float velocity, float elasticity = 0.8f)
        {
            acceleration = Mathf.Clamp(acceleration, 0f, 1f);
            elasticity = Mathf.Clamp(elasticity, 0f, 1f);

            float newValue = value + (target - value) * acceleration + velocity * elasticity;
            velocity = newValue - value;

            return newValue;
        }

        /// <summary>
        /// Lerps towards the target value with a wobbling effect.
        /// </summary>
        /// <param name="value">The value to lerp.</param>
        /// <param name="target">The value to lerps towards.</param>
        /// <param name="acceleration">The acceleration factor, clamped in-between 0 and 1. It should be time dependant.</param>
        /// <param name="velocity">Velocity must be passed as a ref as it is edited each time this function is called.</param>
        /// <param name="elasticity">A value clamped in-between 0 and 1 that drives the wobble effect near the target value. The higher it is, the bigger is the wobble.</param>
        /// <returns></returns>
        public static Vector2 ElasticLerp(Vector2 value, Vector2 target, float acceleration, ref Vector2 velocity, float elasticity = 0.8f)
        {
            acceleration = Mathf.Clamp(acceleration, 0f, 1f);
            elasticity = Mathf.Clamp(elasticity, 0f, 1f);

            Vector2 newValue = value + (target - value) * acceleration + velocity * elasticity;
            velocity = newValue - value;

            return newValue;
        }

        /// <summary>
        /// Lerps towards the target value with a wobbling effect.
        /// </summary>
        /// <param name="value">The value to lerp.</param>
        /// <param name="target">The value to lerps towards.</param>
        /// <param name="acceleration">The acceleration factor, clamped in-between 0 and 1. It should be time dependant.</param>
        /// <param name="velocity">Velocity must be passed as a ref as it is edited each time this function is called.</param>
        /// <param name="elasticity">A value clamped in-between 0 and 1 that drives the wobble effect near the target value. The higher it is, the bigger is the wobble.</param>
        /// <returns></returns>
        public static Vector3 ElasticLerp(Vector3 value, Vector3 target, float acceleration, ref Vector3 velocity, float elasticity = 0.8f)
        {
            acceleration = Mathf.Clamp(acceleration, 0f, 1f);
            elasticity = Mathf.Clamp(elasticity, 0f, 1f);

            Vector3 newValue = value + (target - value) * acceleration + velocity * elasticity;
            velocity = newValue - value;

            return newValue;
        }

        /// <summary>
        /// Lerps towards the target value with a wobbling effect.
        /// </summary>
        /// <param name="value">The value to lerp.</param>
        /// <param name="target">The value to lerps towards.</param>
        /// <param name="acceleration">The acceleration factor, clamped in-between 0 and 1. It should be time dependant.</param>
        /// <param name="velocity">Velocity must be passed as a ref as it is edited each time this function is called.</param>
        /// <param name="elasticity">A value clamped in-between 0 and 1 that drives the wobble effect near the target value. The higher it is, the bigger is the wobble.</param>
        /// <returns></returns>
        public static Quaternion ElasticLerp(Quaternion value, Quaternion target, float acceleration, ref Quaternion velocity, float elasticity = 0.8f)
        {
            acceleration = Mathf.Clamp(acceleration, 0f, 1f);
            elasticity = Mathf.Clamp(elasticity, 0f, 1f);

            Quaternion accelerationDelta = Quaternion.Slerp(Quaternion.identity, target * Quaternion.Inverse(value), acceleration);
            Quaternion velocityDelta = Quaternion.Slerp(Quaternion.identity, velocity, elasticity);
            Quaternion newValue = velocityDelta * accelerationDelta * value;
            velocity = newValue * Quaternion.Inverse(value);

            return newValue;
        }
    }
}