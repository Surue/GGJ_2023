// Old Skull Games
// Benoit Constantin
// Monday, February 25, 2019


//NOT USED FOR THE MOMENT
using UnityEngine;
using UnityEditor;

namespace OSG
{
    public class EditorCurveSettings
    {
        public static AnimationCurve animationCurve;

        public static Color curveBackgroundColor = Color.black;
        public static Color curveGridColor = Color.white;

        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;
        public static void Load()
        {
            curveBackgroundColor = new Color(
                EditorPrefs.GetFloat(nameof(curveBackgroundColor) + "_R"),
                EditorPrefs.GetFloat(nameof(curveBackgroundColor) + "_G"),
                EditorPrefs.GetFloat(nameof(curveBackgroundColor) + "_B"),
                EditorPrefs.GetFloat(nameof(curveBackgroundColor) + "_A"));

            curveGridColor = new Color(
                EditorPrefs.GetFloat(nameof(curveGridColor) + "_R"),
                EditorPrefs.GetFloat(nameof(curveGridColor) + "_G"),
                EditorPrefs.GetFloat(nameof(curveGridColor) + "_B"),
                EditorPrefs.GetFloat(nameof(curveGridColor) + "_A"));

            prefsLoaded = true;
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider Settings()
        {
            return new SettingsProvider("OSG/EditorCurve", SettingsScope.User) {guiHandler = s => OnGUI()};
        }

#else
        [PreferenceItem("Editor Curve")]
#endif
        public static void OnGUI()
        {
            // Load the preferences
            if (!prefsLoaded)
            {
                Load();
            }

            // Preferences GUI
            curveBackgroundColor = EditorGUILayout.ColorField(nameof(curveBackgroundColor), curveBackgroundColor);
            curveGridColor = EditorGUILayout.ColorField(nameof(curveGridColor), curveGridColor);
            animationCurve = EditorGUILayout.CurveField(animationCurve);

            // Save the preferences
            if (GUI.changed)
            {
                EditorPrefs.SetFloat(nameof(curveBackgroundColor) + "_R", curveBackgroundColor.r);
                EditorPrefs.SetFloat(nameof(curveBackgroundColor) + "_G", curveBackgroundColor.g);
                EditorPrefs.SetFloat(nameof(curveBackgroundColor) + "_B", curveBackgroundColor.b);
                EditorPrefs.SetFloat(nameof(curveBackgroundColor) + "_A", curveBackgroundColor.a);


                EditorPrefs.SetFloat(nameof(curveGridColor) + "_R", curveGridColor.r);
                EditorPrefs.SetFloat(nameof(curveGridColor) + "_G", curveGridColor.g);
                EditorPrefs.SetFloat(nameof(curveGridColor) + "_B", curveGridColor.b);
                EditorPrefs.SetFloat(nameof(curveGridColor) + "_A", curveGridColor.a);
            }
        }
    }
}

