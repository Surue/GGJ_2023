// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using OSG.DebugTools;
using UnityEngine;

namespace OSG
{
    public static class Matrix4x4Extensions
    {
        public static Matrix4x4 Sub(this Matrix4x4 m0, Matrix4x4 m1)
        {
            return new Matrix4x4
            {
                m00 = m0.m00 - m1.m00,
                m10 = m0.m10 - m1.m10,
                m20 = m0.m20 - m1.m20,
                m30 = m0.m30 - m1.m30,
                m01 = m0.m01 - m1.m01,
                m11 = m0.m11 - m1.m11,
                m21 = m0.m21 - m1.m21,
                m31 = m0.m31 - m1.m31,
                m02 = m0.m02 - m1.m02,
                m12 = m0.m12 - m1.m12,
                m22 = m0.m22 - m1.m22,
                m32 = m0.m32 - m1.m32,
                m03 = m0.m03 - m1.m00,
                m13 = m0.m13 - m1.m10,
                m23 = m0.m23 - m1.m20,
                m33 = m0.m33 - m1.m30
            };
        }

        public static Quaternion Rotation(this Matrix4x4 m)
        {
            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm

            //Quaternion q = new Quaternion(
            //    Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2]))*0.5f,
            //    Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2]))*0.5f,
            //    Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2]))*0.5f,
            //    Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2]))*0.5f);

            Quaternion q = new Quaternion(
                Mathf.Sqrt(Mathf.Max(0, 1 + m.m00 - m.m11 - m.m22)) * 0.5f,
                Mathf.Sqrt(Mathf.Max(0, 1 - m.m00 + m.m11 - m.m22)) * 0.5f,
                Mathf.Sqrt(Mathf.Max(0, 1 - m.m00 - m.m11 + m.m22)) * 0.5f,
                Mathf.Sqrt(Mathf.Max(0, 1 + m.m00 + m.m11 + m.m22)) * 0.5f);

            q.x *= Mathf.Sign(q.x * (m.m21 - m.m12));
            q.y *= Mathf.Sign(q.y * (m.m02 - m.m20));
            q.z *= Mathf.Sign(q.z * (m.m10 - m.m01));
            return q;
        }

        public static Vector3 Position(this Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static Vector3 Scale(this Matrix4x4 m)
        {
            return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }
    }
}