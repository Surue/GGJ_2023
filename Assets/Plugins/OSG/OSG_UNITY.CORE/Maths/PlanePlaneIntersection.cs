
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    /// <summary>
    /// based on https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrPlane3Plane3.h
    /// only, our planes definitions are using opposite sign constant to their's
    /// </summary>
    public class PlanePlaneIntersection
    {
        public enum Type
        {
            None,
            Line,
            Plane
        }

        public readonly Type type;
        public readonly Vector3 O;  // point 
        public readonly Vector3 D;  // vector

        public PlanePlaneIntersection(Plane p0, Plane p1)
        {
            float dot = Vector3.Dot(p0.normal, p1.normal);
            if(Mathf.Abs(dot)>=0.9999f)
            {
                // planes are parallel
                O = Vector3.zero;
                D = Vector3.zero;

                float diff = p0.distanceToOrigin - Mathf.Sign(dot) * p1.distanceToOrigin;
                if(diff*diff < 0.000001f)
                {
                    // planes are coplanar
                    type = Type.Plane;
                    return;
                }
                type = Type.None;
                return;
            }

            type = Type.Line;
            float invDet = 1 / (1 - dot*dot);
            float c0 = (dot * p1.distanceToOrigin - p0.distanceToOrigin);
            float c1 = (dot * p0.distanceToOrigin - p1.distanceToOrigin);
            O = (c0*p0.normal + c1*p1.normal)*invDet;
            D = Vector3.Cross(p0.normal, p1.normal).normalized;
        }

#if UNITY_EDITOR
        public void DrawGizmo()
        {
            if(type != Type.Line)
                return;
            Vector3 d100 = 100*D;
            Handles.DotHandleCap(0, O, Quaternion.identity, 0.01f, Event.current.type);
            Handles.DrawLine(O - d100, O +d100);
        }
#endif


    }




}