
using System;
using UnityEditor;
using UnityEngine;

namespace OSG
{


    [InitializeOnLoad]
    public static class Arcball
    {

#if UNITY_EDITOR_WIN
        //https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
#else
    public static short GetAsyncKeyState(int vKey)
    {
        return 0;
    }
#endif

        [EditorPrefs] static bool active;
        static Arcball()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        static Vector3? startDirection;
        private static float worldRadius;
        private static Quaternion startRotation;
        private static bool canRotate;
        private static Vector2 mousePosition;
        private static bool startedOnSphere;

        private static void OnSceneGUI(SceneView sceneview)
        {
            if(!active)
                return;

            if(sceneview.in2DMode)
                return;

            Event current = Event.current;

            //if(current.isMouse)
            mousePosition = current.mousePosition;

            if(current.type == EventType.Repaint && (current.control || startDirection.HasValue))
            {
                Handles.color = new Color(0.5f,0.5f,1f,0.66f);
                worldRadius = ComputeWorldRadius(sceneview);
                Handles.DrawWireDisc(sceneview.pivot, sceneview.camera.transform.forward, worldRadius);
                return;
            }

            if(GetAsyncKeyState(0x06)==0 || EditorApplication.isCompiling)
            {
                StopArcBall(sceneview);
            }
            else 
            {
                StartArcBall(sceneview, mousePosition);
                DoArcball(sceneview, mousePosition, GetAsyncKeyState(0x10) != 0);
            }

            if((GetAsyncKeyState(0x05)!=0))
            {
                Transform activeTransform = Selection.activeTransform;
                if (activeTransform)
                {
                    sceneview.LookAtDirect(activeTransform.position, activeTransform.rotation);
                    sceneview.Repaint();
                    current.Use();
                }
            }
        }

        private static float ComputeWorldRadius(SceneView sceneview)
        {
            Camera camera = sceneview.camera;
            if(camera.orthographic)
            {
                Vector3 w = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth,camera.pixelHeight,0));
                w -= camera.ScreenToWorldPoint(Vector3.zero);
                w = camera.transform.InverseTransformVector(w);
                return 0.5f * 0.75f * Math.Min(Mathf.Abs(w.x), Mathf.Abs(w.y));
            }

            return sceneview.cameraDistance * 0.5f * 0.75f;
        }

        private static Quaternion applyQ;
        private enum ConstrainAxis
        {
            None, X,Y,Z
        };


        private static ConstrainAxis constrainAxis;
        private static void DoArcball(SceneView sceneview, Vector2 currentMousePosition, bool constrain)
        {
            bool isOnSphere;
            Vector3 currentDirection = MouseToCamera(sceneview, currentMousePosition, out isOnSphere);

            if(isOnSphere != startedOnSphere)
                return;

            applyQ = BuildQuaternion(currentDirection, startDirection.Value);
            if(!constrain)
            {
                constrainAxis = ConstrainAxis.None;
            }

            if(constrain)
            {
                Vector3 e = applyQ.eulerAngles.NormalizeEulerAngles();

                switch (constrainAxis)
                {
                    case ConstrainAxis.None:
                        constrainAxis = GetConstrainAxis(e);
                        break;
                    case ConstrainAxis.X:
                        e.y = e.z = 0;
                        applyQ.eulerAngles = e;
                        break;
                    case ConstrainAxis.Y:
                        e.z = e.x = 0;
                        applyQ.eulerAngles = e;
                        break;
                    case ConstrainAxis.Z:
                        e.x = e.y = 0;
                        applyQ.eulerAngles = e;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //sceneview.LookAtDirect(sceneview.pivot, startRotation * applyQ);

            sceneview.rotation = startRotation * applyQ;
            if(!isOnSphere)
            {
                startDirection = currentDirection;
                startRotation = sceneview.rotation;
            }
            sceneview.Repaint();
        }

        private static Quaternion BuildQuaternion(Vector3 currentDirection, Vector3 startDirection)
        {
            Vector3 normalized1 = startDirection.normalized;
            Vector3 normalized2 = currentDirection.normalized;
            Vector3 axis = Vector3.Cross(normalized2, normalized1);
            float dot = Vector3.Dot(normalized1, normalized2);
            float angle = Mathf.Atan2(axis.magnitude, dot)* Mathf.Rad2Deg;// * Mathf.Sign(-Vector3.Dot(forward, axis));
            return Quaternion.AngleAxis(angle , axis);
        }

        private static ConstrainAxis GetConstrainAxis(Vector3 e)
        {
            float x = Mathf.Abs(e.x);
            float y = Mathf.Abs(e.y);
            float z = Mathf.Abs(e.z);

            if(x+y+z < 5)
                return ConstrainAxis.None;

            if(x > z)
            {
                return x > y ? ConstrainAxis.X : ConstrainAxis.Y;
            }

            return y > z ? ConstrainAxis.Y : ConstrainAxis.Z;
        }


        private static Vector3 MouseToCamera(SceneView sceneview, Vector2 currentMousePosition, out bool isOnSphere)
        {
            Camera camera = sceneview.camera;
            Transform transform = camera.transform;
            
            currentMousePosition.y = camera.pixelHeight - currentMousePosition.y;
            Ray ray = camera.ScreenPointToRay(currentMousePosition);
            ray.origin = transform.InverseTransformPoint(ray.origin);
            ray.direction = transform.InverseTransformDirection(ray.direction);
            
            Vector3  center = sceneview.camera.transform.InverseTransformPoint(sceneview.pivot);
            RaySphereIntersection inter = new RaySphereIntersection(ray.origin, ray.direction, center, worldRadius);
            Vector3 p;

            isOnSphere = inter.type == RaySphereIntersection.eType.InFront;
            if(isOnSphere )
            {
                p = inter.I;
            }
            else
            {
                float factor = center.z / ray.direction.z;
                //Plane plane = Plane.SphereBackPlaneSeenFromPosition(Vector3.zero, center, worldRadius);
                //float factor = Mathf.Abs(plane.distanceToOrigin) / ray.direction.z;
                p = ray.origin + ray.direction * factor;
            }
            return  p - center;
        }

        private static void StopArcBall(SceneView sceneview)
        {
            if (!startDirection.HasValue) return;
            startDirection = null;
        }

        private static void StartArcBall(SceneView sceneview, Vector2 currentMousePosition)
        {
            if(startDirection.HasValue)
                return;
            worldRadius = ComputeWorldRadius(sceneview);
            startRotation = sceneview.rotation;
            startDirection = MouseToCamera(sceneview, currentMousePosition, out startedOnSphere);
        }
    }
}