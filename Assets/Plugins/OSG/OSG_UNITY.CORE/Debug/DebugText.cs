using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG.DebugTools
{
    /// <summary>
    /// Kinda like UnityEngine.Debug.DrawLine, but with text
    /// </summary>
    public class DebugText : AutoSingleton<DebugText>
    {
        struct TextEntry
        {
            public string text;
            public float deathDate;
            public Vector3 position;
            public Color color;
            public bool shadow;
        }

#if UNITY_EDITOR
        private List<TextEntry> entries = new List<TextEntry>();
#endif

        [Conditional("UNITY_EDITOR")]
        private void ResetInternal()
        {
#if UNITY_EDITOR
            entries.Clear();
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void Reset()
        {
            if (instance)
            {
                instance.ResetInternal();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void AddTextInternal(TextEntry entry)
        {
#if UNITY_EDITOR
            entries.Add(entry);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawText(string text, Vector3 position, Color textColor, bool shadow)
        {
#if UNITY_EDITOR
            Style.normal.textColor = textColor;
            Style.normal.background = shadow ? ShadowBackground : null;
            Handles.Label(position, text, Style);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        internal static void AddText(string text, Vector3 position, Color color, float duration, bool shadow)
        {
            if (duration < Time.fixedDeltaTime)
            {
                duration = Time.fixedDeltaTime;
            }
            Instance.AddTextInternal(new TextEntry()
            {
                text = text,
                position = position,
                color = color,
                deathDate = duration > 0 ? duration + Time.time : 0,
                shadow = shadow
            });
        }

#if UNITY_EDITOR
        private static GUIStyle style;
        private static GUIStyle Style
        {
            get { return style ?? (style = new GUIStyle()); }
        }

        private static Texture2D shadowBackground;

        private static Texture2D ShadowBackground
        {
            get
            {
                if (shadowBackground)
                    return shadowBackground;
            
                shadowBackground = new Texture2D(1, 1);
                shadowBackground.SetPixels(new[] { new Color(0,0,0,0.25f) });
                shadowBackground.Apply();
                return shadowBackground;
            }
        }


        private void OnDrawGizmos()
        {
            for (int index = entries.Count; --index >= 0;)
            {
                TextEntry textEntry = entries[index];
                DrawText(textEntry.text, textEntry.position, textEntry.color, textEntry.shadow);

                if (textEntry.deathDate <= Time.time)
                {
                    int lastEntry = entries.Count - 1;
                    if (lastEntry != index)
                    {
                        entries[index] = entries[lastEntry];
                    }
                    entries.RemoveAt(lastEntry);
                }
            }
        }

#endif
    }
}
