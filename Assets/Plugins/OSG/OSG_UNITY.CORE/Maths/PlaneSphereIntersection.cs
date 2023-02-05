// Old Skull Games
// Bernard Barthelemy

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    public struct PlaneSphereIntersection
    {
        public enum eType
        {
            None,
            Circle,
            Point
        }

        public readonly eType type;
        public readonly float distance;
        public readonly Vector3 onPlane;
        public readonly float circleRadius;
        public Vector3 normal
        {
            get{ return plane.normal;}
        }

        private Plane plane;

        public PlaneSphereIntersection(Plane p, Vector3 center, float radius)
        {
            distance = p.Distance(center);
            plane = p;
            onPlane = center - distance * p.normal;
            float r2 = radius*radius;
            float d2 = distance*distance;
            if (r2 <= d2)
            {
                type = Math.Abs(distance - radius) < 0.00001f ? eType.Point : eType.None;
                circleRadius = 0;
                return;
            }
            type = eType.Circle;
            circleRadius = Mathf.Sqrt(r2 - d2);
        }
       

        public Vector3 ProjectOnCircle(Vector3 P)
        {
            P = plane.ProjectedPoint(P);
            return onPlane + (P - onPlane).normalized * circleRadius;
        }


#if UNITY_EDITOR
        public void DrawGizmo(float size)
        {
            //Vector3 center = onPlane+distance*plane.normal;
            //Handles.DrawLine(onPlane, center);
            Handles.DotHandleCap(0, onPlane, Quaternion.identity, size, Event.current.type);
            //Handles.Label(0.5f * (onPlane + center), distance.ToString("0.0"));
            if(type == eType.Circle)
            {
                Handles.DrawWireArc(onPlane, plane.normal, plane.tangent1, 360, circleRadius*1.005f);
            }
        }
#endif
    }
}

