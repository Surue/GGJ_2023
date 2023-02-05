// Old Skull Games
// Bernard Barthelemy
// Friday, July 28, 2017

//#define SHOW_LOG 

using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSG
{
    /// <summary>
    /// Some helpers function 
    /// </summary>
    public static class UnityObjectEditorExtensions
    {
        [Conditional("SHOW_LOG")]
        static void Log(string m)
        {
            //UnityEngine.Debug.Log(m);
        }

        /// <summary>
        /// find or create an inspector for given UnityEngine.Object
        /// Note : upon return, the inspector is not garanted to
        /// edit the target yet, some async mumbo jumbo is taking
        /// place later in unity's entrails
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static EditorWindow GetInspector(this Object target)
        {
            // Is there some already created inspector window somewhere 
            // that could possibly already edit the Object
            Log("LOOK INTO EXISTING INSPECTORS".InColor(Color.blue));

            EditorWindow w = FindInspectorOf(target);
            if (w) return w;
            // ok, maybe we can use an inspector that's free to be used (not locked)
            // so, let's have the active Selection be on our target
            Selection.activeObject = target;
            w = GetNotLockedInspectorWindow();
            return w;
        }

        public static EditorWindow GetNotLockedInspectorWindow()
        {
            Assembly a = Assembly.GetAssembly(typeof(EditorWindow));
            Type inspectorType = a.GetType("UnityEditor.InspectorWindow");
            var windows = Resources.FindObjectsOfTypeAll(inspectorType);

            // Find a non locked one
            EditorWindow w;
            for (int i = windows.Length ; --i>=0;)
            {
                ActiveEditorTracker tracker = GetTracker(inspectorType, windows[i]);
                if (tracker == null) continue;
                if (tracker.isLocked) continue;
                w = windows[i] as EditorWindow;
                w.Show(true);
                return w;
            }

            // no unlocked inspector, create a brand new one...
            w =  ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
            w.Show(true);
            return w;
        }

        /// <summary>
        /// Finds or creates Inspector for given object and gives it the focus
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static EditorWindow FocusInspector(this Object target)
        {
            var w = target.GetInspector();
            if (w)
            {
                w.Focus();
            }
            return w;
        }

        /// <summary>
        /// Find an inspector for target object
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static EditorWindow FindInspectorOf(Object target)
        {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                Log("Window " + window.name);
                if (window.IsEditing(target)) return window;
            }
            return null;
        }

        /// <summary>
        /// is that window currently editing this object ?
        /// </summary>
        /// <param name="window"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsEditing(this EditorWindow window, Object target)
        {
            Type t = window.GetType();
            if (t.Name != "InspectorWindow") return false;
            ActiveEditorTracker tracker = GetTracker(t, window);
            if (tracker == null) return false;
            bool isEditing = false;

#if SHOW_LOG
            string logText = window.titleContent.text + " edits : ";
#endif
            foreach (var editor in tracker.activeEditors)
            {
#if SHOW_LOG
                logText += " " + editor.target.name;
#endif
                if (ReferenceEquals(editor.target,target))
                {
#if SHOW_LOG
                    Log(logText.InColor(Color.green));
#endif
                    isEditing =true;
                }
            }
#if SHOW_LOG
            Log(logText.InColor(Color.red));
#endif
            return isEditing;
        }

        private static ActiveEditorTracker GetTracker(Type type, object window)
        {
            FieldInfo info = type.GetField("m_Tracker", BindingFlags.Instance|BindingFlags.NonPublic);
            if (info == null) return null;
            return  info.GetValue(window) as ActiveEditorTracker;

        }
    }
}