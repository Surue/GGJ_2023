using System.Linq;
using UnityEngine;
using UnityEditor;

namespace OSG
{
    public abstract class EditorSingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (!_instance)
                    _instance = GetAsset();

                // Asset has not been found
                if (!_instance && Application.isPlaying)
                {
                    Debug.LogError($"No <color=red>{typeof(T)}</color> found in any Editor folder of the project.");
                }

                return _instance ? _instance : null;
            }
        }

        private static T GetAsset()
        {
            string[] editorPaths = AssetDatabase.FindAssets("t:folder Editor");
            for (int e = 0; e < editorPaths.Length; e++)
            {
                editorPaths[e] = AssetDatabase.GUIDToAssetPath(editorPaths[e]);
            }

            string[] paths = AssetDatabase.FindAssets($"t:{typeof(T).Name}", editorPaths);
            var assets = paths.Select(t => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(t), typeof(T)) as T).ToList();
            if (assets.Count > 0)
            {
                if(assets.Count>1)
                {
                    Debug.LogWarning($"Found multiple assets for {typeof(T).FullName} using {AssetDatabase.GetAssetPath(assets[0])}");
                }
                return assets[0];
            }
            // now is our chance to find a default version in our assembly (that would be named "Default{Type.Name}" )

            return AssetDatabase.LoadAssetAtPath<T>($"Packages/com.osg.unitycore.editor/DefaultResources/Default{typeof(T).Name}.asset");
        }
    }
}