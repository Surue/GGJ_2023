// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using UnityEditor;

namespace OSG
{
    /// <summary>
    /// Forbids the editor to update when the game is in PIE.
    /// So you won't break your current game if you modify some
    /// asset or source code.
    /// </summary>
    [InitializeOnLoad]
    public static class DisableAutoRefreshInPlayMode
    {
        static DisableAutoRefreshInPlayMode()
        {
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += OnPlayModeChange;
#else
            EditorApplication.playmodeStateChanged += OnPlayModeChange;
#endif
        }
#if UNITY_2017_2_OR_NEWER
        private static void OnPlayModeChange(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    EditorPrefs.SetBool("kAutoRefresh", true);
                    EditorListener.OnNextUpdateDo(UpdatePreferencesWindow);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingEditMode:
                    EditorPrefs.SetBool("kAutoRefresh", false);
                    break;
            }
        }
        private static void UpdatePreferencesWindow()
        {
            Type preferenceWindowType = Type.GetType("UnityEditor.PreferencesWindow,UnityEditor.dll");
            if(preferenceWindowType!=null)
            {
                var all = UnityEngine.Resources.FindObjectsOfTypeAll(preferenceWindowType);
                EditorWindow window = (EditorWindow) (all.Length>0? all[0] : null);
                if (window)
                {
                    window.Close();
                    EditorWindow.GetWindow(preferenceWindowType, true);
                }
            }
            AssetDatabase.Refresh();
        }
#else
        private static void OnPlayModeChange()
        {
            bool allowAutoRefresh = !(EditorApplication.isPlayingOrWillChangePlaymode
                                      || EditorApplication.isPlaying
                                      || EditorApplication.isPaused);
            
            EditorPrefs.SetBool("kAutoRefresh", allowAutoRefresh);
            if (allowAutoRefresh)
            {
                EditorListener.OnNextUpdateDo(AssetDatabase.Refresh);
            }
        }
#endif
    }
}
