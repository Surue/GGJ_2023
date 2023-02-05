// Old Skull Games
// Bernard Barthelemy
// Thursday, August 30, 2018

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OSG
{
    class AndroidBuildHelper : IPreprocessBuildWithReport
    {

        #pragma warning disable 649
        [EditorPrefs] private static string storePassWord;
        [EditorPrefs] private static string keyPassWord;
        [EditorPrefs] private static bool storePassword;
        [EditorPrefs] private static string storePath;
        [EditorPrefs] private static string keyName;
        #pragma warning restore 649
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            OnPreprocessBuild(report.summary.platform, "");
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.Android)
                return;
            if(storePassword)
            {
#if UNITY_2019_1_OR_NEWER
                PlayerSettings.Android.useCustomKeystore = true;
#endif
                PlayerSettings.Android.keyaliasPass = keyPassWord;
                PlayerSettings.Android.keystorePass = storePassWord;
                string keypath = Application.dataPath.Replace("/Assets", "");
                keypath = DirectoryHelpers.Combine(keypath, storePath);
                PlayerSettings.Android.keystoreName = keypath;
                PlayerSettings.Android.keyaliasName = keyName;
            }
        }
    }
}
