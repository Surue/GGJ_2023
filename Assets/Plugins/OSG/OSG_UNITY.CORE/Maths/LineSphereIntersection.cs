

using System;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    public class LineSphereIntersection
    {
        public enum eType
        {
            None,
            OnePoint,
            TwoPoints
        }

        public readonly eType type;
        public readonly Vector3 I0;
        public readonly Vector3 I1;
        public readonly float t0;
        public readonly float t1;

        /// <summary>
        /// Get the intersection 
        /// </summary>
        /// <param name="O">ray origin</param>
        /// <param name="D">ray direction</param>
        /// <param name="C">sphere center</param>
        /// <param name="R">sphere radius</param>
        public LineSphereIntersection(Vector3 O, Vector3 D, Vector3 C, float R)
        {
            float a = 2*D.sqrMagnitude;
            Vector3 CO = O-C;
            float b = -2 *  Vector3.Dot(D, CO);
            float c = CO.sqrMagnitude - R*R;
            float delta = b*b - 2*a*c;
            if(delta<0)
            {
                type = eType.None;
                I0 = I1 = Vector3.positiveInfinity;
                t0 = t1 = float.PositiveInfinity;
                return;
            }
                
            if(Math.Abs(delta) < 0.0001f)
            {
                t0 = t1 = b / a;
                type = eType.OnePoint;
                I0 = I1 = O + t0*D;
            }
            else
            {
                type = eType.TwoPoints;
                float s = Mathf.Sqrt(delta);
                t0 = (b - s)/a;
                I0 = O + t0 * D;
                t1 = (b + s)/a;
                I1 = O + t1 * D;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawGizmo(float size, string s)
        {
#if UNITY_EDITOR
            if(type==eType.None)
            {
                return;
            }

            Handles.DotHandleCap(0, I0, Quaternion.identity, size, Event.current.type);
            
            if(type == eType.TwoPoints)
            {
                Handles.DotHandleCap(0, I1, Quaternion.identity, size, Event.current.type);
                Handles.Label(I0, s + "0");
                Handles.Label(I1, s + "1");
            }
            else
            {
                Handles.Label(I0, s);
            }
#endif
        }

    }
}