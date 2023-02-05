using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Path to a scene asset. Managed in editor by ScenePathPropertyDrawer
/// </summary>
[Serializable]
public class ScenePath : IEquatable<ScenePath>
{
    [FormerlySerializedAs("scene")]
    public string path;

    public AsyncOperation Load(LoadSceneMode loadSceneMode)
    {
        return SceneManager.LoadSceneAsync(path,loadSceneMode);
    }

    public override string ToString()
    {
        return path;
    }

    public static bool IsSceneValidButNotLoaded(ScenePath scenePath)
    {
        if(string.IsNullOrEmpty(scenePath?.path)) return false;

        Scene sceneByPath = SceneManager.GetSceneByPath(scenePath.path);
        return sceneByPath.IsValid() && !sceneByPath.isLoaded;
    }

#if UNITY_EDITOR
    // ReSharper disable once UnusedMember.Global // used in editor
    public string guid;
    public Scene Open()
    {
        Scene opened = SceneManager.GetSceneByPath(path);
        if (!opened.isLoaded)
        {
            try
            {
                return EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
        return opened;
    }


    public bool OnGui(Rect rect)
    {
        SceneAsset sa = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        SceneAsset newScene = EditorGUI.ObjectField(rect, sa, typeof(SceneAsset), false) as SceneAsset;
        if(newScene != sa)
        {
            path = AssetDatabase.GetAssetPath(newScene);
            guid = AssetDatabase.AssetPathToGUID(path);
            return true;
        }

        return false;
    }

#endif
    public bool Equals(ScenePath other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || string.Equals(path, other.path);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((ScenePath) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (path != null ? path.GetHashCode() : 0) * 397;
        }
    }
}