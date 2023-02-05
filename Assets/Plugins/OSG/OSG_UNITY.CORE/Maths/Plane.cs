// Old Skull Games
// Bernard Barthelemy

using System;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace OSG
{
    public struct Plane
    {
        // p is in plane iff n.x * p.x + n.y * p.y + n.z * p.z + d = 0
        public readonly Vector3 normal;
        public readonly float distanceToOrigin;
        // t1 and t2 are 2 other vectors in the plane which with n, form an orthogonal base.
        public readonly Vector3 tangent1;
        public readonly Vector3 tangent2;
        public readonly Vector3 onPlane;

        private Vector3 namePos;
        private string name;

        public void SetName(Vector3 position, string name)
        {
            namePos = position;
            this.name = name;
        }

        private Vector3[] P;

        /// <summary>
        /// Build a plane from 3 points
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Plane(Vector3 p0, Vector3 p1, Vector3 p2) : this(GetNormal(p0,p1,p2), p0)
        {
            P = new[]{p0,p1,p2,p0};
        }

        /// <summary>
        /// Make a plane from a normal and a point
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="point"></param>
        public Plane(Vector3 normal, Vector3 point)
        {
            P = null;
            this.normal = normal.normalized;
            distanceToOrigin = -Vector3.Dot(this.normal, point);
            onPlane = point;
            name = "";
            namePos = Vector3.zero;
            Vector3 t = this.normal.MostNormalAxis();
            tangent1 = Vector3.Cross(t,this.normal).normalized;
            tangent2 = Vector3.Cross(tangent1, this.normal).normalized;
            project=null;
        }
        /// <summary>
        /// Get the projection of a point on the plane
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public Vector3 ProjectedPoint(Vector3 P)
        {
            return P - Distance(P) * normal;
        }


        private Matrix4x4? project;

        public Vector3 ProjectedVector(Vector3 v)
        {
            project = project?? Matrix4x4.identity.Sub(normal.OuterProduct(normal)); 
            return project.Value.MultiplyVector(v);
        }

        public Vector3? ProjectPointAlongDirection(Vector3 point, Vector3 direction)
        {
            float vn = Vector3.Dot(normal, direction);
            if (vn*vn < float.Epsilon)
            {
                return null;
            }

            float nop = Vector3.Dot(normal, point);
            float k = (nop + distanceToOrigin) / vn;
            return point - k * direction;
        }

        /// <summary>
        /// Distance of p to plane
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public float Distance(Vector3 p)
        {
            float dot = Vector3.Dot(normal,p);
            return  dot + distanceToOrigin;
        }

        /// <summary>
        /// Computes the plane, which intersection with the sphere 
        /// of center C and radius r is the circle enclosing said sphere as 
        /// seen from O
        /// see (1) https://www.mathopenref.com/consttangents.html
        /// and (2) http://mathworld.wolfram.com/Sphere-SphereIntersection.html
        /// for the method
        /// </summary>
        /// <param name="O"></param>
        /// <param name="C"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Plane SphereBackPlaneSeenFromPosition(Vector3 O, Vector3 C, float r)
        {
            // 
            // The plane is containing the intersection of 2 spheres
            // - the first one S1 centered on C of radius r
            // - the second one S2 centered on M, of radius R
            // M is the middpoint of OC
            // R is half the distance OC (the seccond one contains both O and C 

            Vector3 OC = C-O;
            float mag = OC.magnitude;
            Vector3 normal = OC/mag;
            float R = mag * 0.5f;
            float x = (mag*R - r*r)/mag;
            Vector3 M = Vector3.Lerp(O,C,0.5f);
            Vector3 I = M + x * normal;
            return new Plane(-normal, I);
        }

        public Vector3 SymetricPoint(Vector3 P)
        {
            return P - 2*Distance(P) * normal;
        }
   
        [Conditional("UNITY_EDITOR")]
        public void DrawGizmo(float size)
        {
#if UNITY_EDITOR
            if(P != null)
            {
                Handles.DrawDottedLines(P, 16);
            }
            else
            {
                Vector3 t1 = size * tangent1;
                Vector3 t2 = size * tangent2;

                Vector3 p0 = onPlane - t1 - t2;
                var points = new Vector3[]
                {
                p0,
                onPlane - t1 + t2,
                onPlane + t1 + t2,
                onPlane + t1 - t2,
                p0
                };

                Quaternion q = new Quaternion();
                q.SetLookRotation(normal);
                Handles.ArrowHandleCap(0, onPlane, q, size * 0.025f, Event.current.type);
                Color color = Handles.color;
                color.a = 0.075f;

                Handles.DrawSolidRectangleWithOutline(points, color, Handles.color);
            }
            if (!string.IsNullOrEmpty(name))
            {
                Handles.Label(namePos, name);
            }
#endif
        }

        #region private stuff
        private static Vector3 P0Px(Vector3 p0, Vector3 px)
        {
            Vector3 p0px = px-p0;
            float m = p0px.magnitude;
            if(m < 0.001f)
            {
                throw new ArgumentException("2 points are too close");
            }
            return p0px/m;
        }


        private static Vector3 GetNormal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 p0p1 = P0Px(p0, p1);
            Vector3 p0p2 = P0Px(p0, p2);

            float dot = Mathf.Abs(Vector3.Dot(p0p1,p0p2));
            if(dot>0.999f)
            {
                throw new ArgumentException("The given points are aligned");
            }
            // p1 and p2 can't be too close to each other at this point,
            // otherwise the dot test would fail

            // n is normal to the plane, so just get it
            return Vector3.Cross(p0p1, p0p2).normalized;
        }

        #endregion


  }
}

