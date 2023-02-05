using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

namespace OSG.DebugTools
{
    /// <summary>
    /// Holds the data for Debug History Steps
    /// </summary>
    public class DebugHistory : OSGMono
    {
        [Conditional("UNITY_EDITOR")]
        internal static void AddEntry(IDebugHistoryStep entry)
        {
#if UNITY_EDITOR
            Instance.AddEntryInternal(entry);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void Clear()
        {
#if UNITY_EDITOR
            Instance.ClearInternal();
#endif
        }

#if UNITY_EDITOR
        private static DebugHistory instance;
        [Range(0.01f, 10)]
        public float gizmoSize = 1;

        private static DebugHistory Instance
        {
            get
            {
                if (instance) return instance;
                instance = FindObjectOfType<DebugHistory>();
                if (instance) return instance;
                instance = new GameObject("DebugHistory", typeof(DebugHistory)).GetComponent<DebugHistory>();
                if (instance) return instance;
                UnityEngine.Debug.LogError("Can't create DebugHistory instance");
                return null;
            }
        }

        private void AddEntryInternal(IDebugHistoryStep entry)
        {
            entries.Add(entry);
            types.Add(entry.GetType());
        }


        private GUIStyle style;
        private GUIStyle Style
        {
            get { return style ?? (style = new GUIStyle()); }
        }

        private int _currentEntry;

        private int currentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                _currentEntry = value;
                if (entries.Count > 0)
                {
                    _currentEntry = _currentEntry%entries.Count;
                    if (!SceneView.lastActiveSceneView) return;
                    Vector3? poi = entries[_currentEntry].POI();
                    if (poi.HasValue)
                    {
                        SceneView.lastActiveSceneView.pivot = poi.Value;
                    }
                }
                else
                {
                    _currentEntry = 0;
                }
            }

        }
        [UsedImplicitly] public bool showNotSelected;

        private HashSet<Type> types = new HashSet<Type>();

        private void OnDrawGizmos()
        {
            if (entries.Count <= 0)
                return;
            for (int index = entries.Count; --index >= 0;)
            {
                if (index == currentEntry)
                {
                    entries[index].OnDrawGizmosSelected(gizmoSize);
                }
                else if(showNotSelected)
                {
                    entries[index].OnDrawGizmos(gizmoSize);
                }
            }
        }
        public void OnInspectorGUI(SerializedObject serializedObject)
        {
            if (entries == null || entries.Count <= 0)
            {
                GUILayout.Label("Nothing");
                return;
            }
        
            if (entries.Count > 1)
            {
                GUILayout.BeginHorizontal();
                int newEntry = (int) EditorGUILayout.Slider(currentEntry, 0, entries.Count-1);
                GUILayout.EndHorizontal();
                if (newEntry != currentEntry)
                    currentEntry = newEntry;
            }

            GUILayout.BeginHorizontal();
            if (currentEntry>0 && GUILayout.Button("-"))
            {
                --currentEntry;
            }
            if (currentEntry < entries.Count-1 && GUILayout.Button("+"))
            {
                ++currentEntry;
            }
            GUILayout.EndHorizontal();

            foreach (Type type in types)
            {
                Edit(type);
            }
            entries[currentEntry].OnInspectorUI();
            bool deleteEntries = GUILayout.Button("Clear");
            if (deleteEntries)
            {
                ClearInternal();
            }
        }

        private void ClearInternal()
        {
            entries.Clear();
            types.Clear();
            currentEntry = 0;
        }

        private void Edit(Type type)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(type.Name);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("<"))
            {
                int next = FindNextOfType(type, -1);
                if (next >= 0)
                {
                    currentEntry = next;
                }
            }

            if (GUILayout.Button(">"))
            {
                int next = FindNextOfType(type, 1);
                if (next >= 0)
                {
                    currentEntry = next;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private int FindNextOfType(Type type, int direction)
        {
            int nextPos = direction + currentEntry;
            for (;;)
            {
                if (nextPos < 0 || nextPos >= entries.Count)
                {
                    return -1;
                }

                if (entries[nextPos].GetType() == type)
                {
                    return nextPos;
                }
                nextPos += direction;
            }
        }

        private List<IDebugHistoryStep> entries = new List<IDebugHistoryStep>();
#endif

    }
}
