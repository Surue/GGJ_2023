using System;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using Debug = System.Diagnostics.Debug;

namespace OSG.DebugTools
{
    /// <summary>
    /// History Step interface.
    /// An history step is a set of data usefull at a given step of a debugged algorithm.
    /// </summary>
    
    public interface IDebugHistoryStep
    {
        /// <summary>
        /// When the step is currently active, this will allow to draw 
        /// a gizmo in editor's scene
        /// </summary>
        /// <param name="gizmoSize"></param>
        void OnDrawGizmos(float gizmoSize);
        /// <summary>
        /// Same as OnDrawGizmo, but the step is active and selected
        /// </summary>
        /// <param name="gizmoSize"></param>
        void OnDrawGizmosSelected(float gizmoSize);
        /// <summary>
        /// Allow to write the steps' data in the inspector window
        /// </summary>
        void OnInspectorUI();
        /// <summary>
        /// Allow to define a 3D position of Interest for this step
        /// </summary>
        /// <returns></returns>
        Vector3? POI();
    }

    /// <summary>
    /// Base history step that allows to display the call stack at its creation time.
    /// You still have to call the StackInspector from a GUI method, though :p
    /// </summary>
    public abstract class HistoryStepWithStack : IDebugHistoryStep
    {
        public abstract void OnDrawGizmos(float gizmoSize);
        public abstract void OnDrawGizmosSelected(float gizmoSize);
        public abstract void OnInspectorUI();
        public abstract Vector3? POI();
#if UNITY_EDITOR
        private DebugUtils.Source[] sources;
        protected HistoryStepWithStack(int frameIndex)
        {
            sources = DebugUtils.GetCallStack(frameIndex);
        }
        /// <summary>
        /// Should allow to go to the code line where the step has been created.
        /// </summary>
        protected void StackInspector()
        {
            DebugUtils.StackInspector(sources);
        }
#endif
    }
    /// <summary>
    /// Set of Debug utilities 
    /// </summary>
    public static class DebugUtils
    {

        public static void Draw(this Bounds bounds, Color color, float duration = 0)
        {
            Vector3 v0 = bounds.min;
            Vector3 v6 = bounds.max;
            Vector3 v1 = new Vector3(v6.x, v0.y, v0.z);
            Vector3 v2 = new Vector3(v6.x, v6.y, v0.z);
            Vector3 v3 = new Vector3(v0.x, v6.y, v0.z);

            Vector3 v4 = new Vector3(v0.x, v0.y, v6.z);
            Vector3 v5 = new Vector3(v6.x, v0.y, v6.z);
            Vector3 v7 = new Vector3(v0.x, v6.y, v6.z);

            UnityEngine.Debug.DrawLine(v0, v4, color, duration);
            UnityEngine.Debug.DrawLine(v1, v5, color, duration);
            UnityEngine.Debug.DrawLine(v2, v6, color, duration);
            UnityEngine.Debug.DrawLine(v3, v7, color, duration);

            UnityEngine.Debug.DrawLine(v0, v1, color, duration);
            UnityEngine.Debug.DrawLine(v1, v2, color, duration);
            UnityEngine.Debug.DrawLine(v2, v3, color, duration);
            UnityEngine.Debug.DrawLine(v3, v0, color, duration);

            UnityEngine.Debug.DrawLine(v4, v5, color, duration);
            UnityEngine.Debug.DrawLine(v5, v6, color, duration);
            UnityEngine.Debug.DrawLine(v6, v7, color, duration);
            UnityEngine.Debug.DrawLine(v7, v4, color, duration);
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawStar(Vector3 position, Color color, float size=1.0f, float duration=0)
        {
            Vector3 dir = Vector3.up*size;
            UnityEngine.Debug.DrawLine(position + dir, position - dir, color, duration);
            dir = Vector3.right * size;
            UnityEngine.Debug.DrawLine(position + dir, position - dir, color, duration);
            dir = Vector3.forward * size;
            UnityEngine.Debug.DrawLine(position + dir, position - dir, color, duration);
        }
#if UNITY_EDITOR
        const float cosArrow = 0.866025f;
        const float sinArrow = 0.5f;
#endif
        [Conditional("UNITY_EDITOR")]
        public static void DrawArrow(Vector3 position, Vector3 direction, Color color, float duration = 0)
        {
#if UNITY_EDITOR
          //  if(duration == 0)
          //  {
          //      Quaternion q = new Quaternion();
          //      q.SetLookRotation(direction);
          //      Handles.color = color;
          //      float magnitude = direction.magnitude;
          //      EventType type = Event.current!=null ? Event.current.type : EventType.Repaint;
          //      Handles.ArrowHandleCap(0, position, q, magnitude, type);
          //
          //      Handles.color = color;
          //      Handles.ArrowHandleCap(0, position, q, magnitude, type);
          //      return;
          //  }

            Vector3 tip = position+direction;
            UnityEngine.Debug.DrawLine(position, tip, color, duration);
            direction = direction * -0.2f;
            float cx = cosArrow * direction.x;
            float sx = sinArrow * direction.x;

            float cz = cosArrow * direction.z;
            float sz = sinArrow * direction.z;

            float cy = cosArrow * direction.y;
            float sy = sinArrow * direction.y;

            Vector3 p0 = new Vector3(cx - sz, direction.y, sx + cz);
            Vector3 p1 = new Vector3(cx + sz, direction.y, cz - sx);
        
            UnityEngine.Debug.DrawLine(tip, tip+p0, color, duration);
            UnityEngine.Debug.DrawLine(tip, tip+p1, color, duration);
        
            p0 = new Vector3(cx - sy, sx + cy, direction.z);
            p1 = new Vector3(cx + sy, cy - sx, direction.z);

            UnityEngine.Debug.DrawLine(tip, tip + p0, color, duration);
            UnityEngine.Debug.DrawLine(tip, tip + p1, color, duration);

            p0 = new Vector3(direction.x, cz - sy, sz + cy);
            p1 = new Vector3(direction.x, cz + sy, cy - sz);

            UnityEngine.Debug.DrawLine(tip, tip + p0, color, duration);
            UnityEngine.Debug.DrawLine(tip, tip + p1, color, duration);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawEllipse(Vector3 position, Vector3 I, Vector3 J, Color color, float duration = 0, int segments = 16)
        {
            float dAngle = 2*Mathf.PI/segments;
            Vector3 pos = position + I;
            float angle = dAngle;
            for (int i = 0; i < segments; ++i)
            {
                float c = Mathf.Cos(angle);
                float s = Mathf.Sin(angle);
                angle += dAngle;
                var newPos = position + c * I + s * J;
                UnityEngine.Debug.DrawLine(pos, newPos, color, duration);
                pos = newPos;
            }
        }


#if UNITY_EDITOR
        public static Vector3 GetWorldLegendPos(out Vector3 delta, Vector3 legendPos)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (!view)
            {
                view = SceneView.currentDrawingSceneView;
                if (!view)
                {
                    delta = Vector3.up;
                    return Vector3.zero;
                }
            }

            Quaternion quaternion = view.rotation;
            float ratio = view.position.width / view.position.height;
            Vector3 pos = view.pivot + quaternion * (view.size * new Vector3(legendPos.x * ratio, legendPos.y, legendPos.z));
            delta = quaternion * new Vector3(0, 0.05f * view.size, 0);
            return pos;
        }
#endif

        [Conditional("UNITY_EDITOR")]
        public static void AddText(string text, Vector3 position, Color color, float duration, bool shadow=true)
        {
            DebugText.AddText(text, position, color, duration, shadow);
        }

        [Conditional("UNITY_EDITOR")]
        internal static void DrawText(string text, Vector3 position, Color color, bool shadow = true)
        {
            DebugText.DrawText(text, position, color, shadow);
        }

        [Conditional("UNITY_EDITOR")]
        public static void ResetText()
        {
            DebugText.Reset();
        }


        [Conditional("UNITY_EDITOR")]
        public static void AddDebugHistory(IDebugHistoryStep entry)
        {
            DebugHistory.AddEntry(entry);
        }

        [Conditional("UNITY_EDITOR")]
        public static void ClearDebugHistory()
        {
            DebugHistory.Clear();
        }


        public static Vector3 GetPositionInSceneEditor()
        {
#if UNITY_EDITOR
            if (SceneView.currentDrawingSceneView)
            {
                return SceneView.currentDrawingSceneView.pivot;
            }
            else if (SceneView.lastActiveSceneView)
            {
                return SceneView.lastActiveSceneView.pivot;
            }
#endif
            return Vector3.zero;
        }

        [Conditional("UNITY_EDITOR")]
        public static void ShowPositionInSceneEditor(Vector3 position)
        {
#if UNITY_EDITOR
            if (SceneView.currentDrawingSceneView)
            {
                SceneView.currentDrawingSceneView.pivot = position;
            }
            else if(SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.pivot = position;
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void OpenFileAtLineInIDE(string fileName, int line)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(fileName))
                return;

            var osFileName = fileName.Replace('/', Path.DirectorySeparatorChar);
            var filename = Path.Combine(Directory.GetCurrentDirectory(), osFileName);
            if(File.Exists(filename))
                   UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filename, line);
#endif
        }

        [Serializable]
        public struct Source
        {
            public int line;
            public string fileName;
            public string callerName;

            private static string Nicify(string name)
            {
                if(name[0] == '<')
                {
                    for(int i =1; i < name.Length; ++i)
                    {
                        if(name[i] == '>')
                            return name.Substring(1, i-1);
                    }
                }
                return name;
            }

            public Source(StackFrame stackFrame)
            {
                line = stackFrame.GetFileLineNumber();
                fileName = stackFrame.GetFileName();
                var callerType = stackFrame.GetMethod().ReflectedType ??
                                 (stackFrame.GetMethod().DeclaringType ?? typeof(object));
                callerName = Nicify(callerType.Name) + "." + Nicify(stackFrame.GetMethod().Name);
            }
        }


        public static Source[] GetCallStack(int frameIndex)
        {
            var trace = new StackTrace(true);
            StackFrame[] frames = trace.GetFrames();
            if (frames == null)
                return null;
            if (frames.Length < frameIndex)
            {
                UnityEngine.Debug.LogError("Not enough frames (only " + frames.Length + " wanted " + frameIndex + ')');
                return null;
            }
            int sourceCount = frames.Length - frameIndex;
            var sources = new Source[sourceCount];
            for (int index = 0; index < sourceCount; ++index)
            {
                sources[index] = new Source(frames[index + frameIndex]);
            }
            return sources;
        }
#if UNITY_EDITOR
        public static void StackInspector(Source[] sources)
        {
            foreach (var source in sources)
            {
                if (GUILayout.Button("   " + source.callerName, GUI.skin.label))
                {
                    DebugUtils.OpenFileAtLineInIDE(source.fileName, source.line);
                }
            }
        }

        public static bool JumpToSource(Source frame)
        {
            if (string.IsNullOrEmpty(frame.fileName))
                return false;

            var osFileName = frame.fileName.Replace('/', Path.DirectorySeparatorChar);
            var filename = Path.Combine(Directory.GetCurrentDirectory(), osFileName);
            return File.Exists(filename) &&
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filename, frame.line);
        }
#endif
    }
}

namespace OSG.Core
{
    public static class CoreDebugInit
    {
#pragma warning disable 414
        private static int init = InitLogAction();
#pragma warning restore 414
        private static int InitLogAction()
        {
            // Initializing the Core Log functions
            CoreDebug.SetLogFunction(UnityEngine.Debug.Log);
            CoreDebug.SetLogWarningFunction(UnityEngine.Debug.LogWarning);
            CoreDebug.SetLogErrorFunction(UnityEngine.Debug.LogError);
            return 1;
        }
    }
}
