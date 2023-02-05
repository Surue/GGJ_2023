using UnityEngine;

namespace OSG
{
    public static class Vector4Extensions
    {
        /// <summary>
        /// Converts a Vector4 to a Quaternion.
        /// </summary>
        public static Quaternion ToQuaternion(this Vector4 vector)
        {
            return new Quaternion(vector.x, vector.y, vector.z, vector.w);
        }
    }
}