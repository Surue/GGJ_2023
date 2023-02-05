//#define USE_DOT

using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    public class RaySphereIntersection
    {
        public enum eType
        {
            None,
            InFront,
            Behind
        }

        public readonly eType type;
        public readonly Vector3 I;
        public readonly float t;

        /// <summary>
        /// Get the intersection 
        /// </summary>
        /// <param name="O">ray origin</param>
        /// <param name="D">ray direction</param>
        /// <param name="C">sphere center</param>
        /// <param name="R">sphere radius</param>
        /// 
#if !USE_DOT
        public RaySphereIntersection(Vector3 O, Vector3 D, Vector3 C, float R)
        {
            Vector3 CO = C-O;
            float a = D.sqrMagnitude;
            a += a;
            float b = Vector3.Dot(D, CO);
            b += b;
            float c = a*(R*R-CO.sqrMagnitude);
            float delta = b*b + c + c;
            if(delta<0)
            {
                type = eType.None;
                return;
            }
            float s = Mathf.Sqrt(delta);
            t = (b < s ? b + s : b - s)/a;
            type = t >= 0 ? eType.InFront : eType.Behind;
            I = O + t*D;
        }
#else
        public RaySphereIntersection(Vector3 O, Vector3 D, Vector3 C, float R)
        {
            Vector3 OC = C-O;
            D.Normalize();
            float x = Vector3.Dot(OC,D);
            float y2 = (x*D - OC).sqrMagnitude; // that's CP, or O + x*D - C, which is O-C + x*D
            float r2 = R*R;
            if(r2 < y2)
            {
                type = eType.None;
                return;
            }
            float ax = Mathf.Sqrt(r2-y2);
            t = x > ax ? x - ax : x + ax;
            type = t >= 0 ? eType.InFront : eType.Behind;
            I = O + D*t;
        }
#endif
        [Conditional("UNITY_EDITOR")]
        public void DrawGizmo(float size, string s)
        {
#if UNITY_EDITOR
            if(type!=eType.InFront)
            {
                return;
            }

            Handles.DotHandleCap(0, I, Quaternion.identity, size, Event.current.type);
            Handles.Label(I, s);
#endif
        }

    }
}