
using UnityEngine;
using UnityEditor;

namespace OSG
{
    public class TimeEditor : EditorWindow
    {
        [MenuItem("OSG/Time Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            TimeEditor window = (TimeEditor) EditorWindow.GetWindow(typeof(TimeEditor));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Time", GUILayout.Width(75));
            float scale = EditorGUILayout.Slider(Time.timeScale, 0, 20);
            if (scale > 0)
            {
                TimeScale.Force(scale);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            float[] timescaleValues = new[] {0.1f, 0.5f, 1f, 5, 10, 20};
            for (var index = 0; index < timescaleValues.Length; index++)
            {
                var aValue = timescaleValues[index];
                float width = (aValue == 1) ? 50 : 30;
                if (GUILayout.Button("" + aValue, GUILayout.Width(width)))
                {
                    if (aValue == 1)
                        TimeScale.Reset();
                    else
                        TimeScale.Force(aValue);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            int frameRate =
                (int) EditorGUILayout.Slider(new GUIContent("Frame Rate"), Application.targetFrameRate, 0, 60);
            if (GUILayout.Button("∞", GUILayout.Width(25)))
            {
                frameRate = 0;
            }

            if (Application.targetFrameRate != frameRate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = frameRate;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            int newVSync = (int) EditorGUILayout.Slider(new GUIContent("VSync"), QualitySettings.vSyncCount, 0, 2);
            if (newVSync != QualitySettings.vSyncCount)
            {
                QualitySettings.vSyncCount = newVSync;
            }

            GUILayout.EndHorizontal();
        }
    }
}