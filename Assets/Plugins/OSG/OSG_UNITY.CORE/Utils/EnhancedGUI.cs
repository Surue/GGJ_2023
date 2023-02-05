using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OSG
{
    public class EnhancedGUI
    {
        // GUI Layout Options
        public static GUILayoutOption[] autoWidth { get { return new GUILayoutOption[2] { GUILayout.Width(0), GUILayout.ExpandWidth(true) }; } }

        // GUI Styles (text)
        public static GUIStyle richText
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    richText = true
                };
            }
        }
        public static GUIStyle labelWrapStyle
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true
                };
            }
        }
        public static GUIStyle richLabelWrapStyle
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    wordWrap = true
                };
            }
        }
        public static GUIStyle categoryTitleStyle
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    richText = true
                };
            }
        }
        public static GUIStyle centeredTextStyle
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
        public static GUIStyle centeredWrapTextStyle
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
            }
        }

        // GUI Styles (button)
        public static GUIStyle centeredButton
        {
            get
            {
                return new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        // Colors
        public static Color prefabOverrideColor = new Color(0.2039216f, 0.6509804f, 0.8941177f, 1);
    }
}