// Old Skull Games
// Bernard Barthelemy
// Tuesday, March 20, 2018


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AssetHelpers
{

    public static T[] GetAtPath<T>(string path)
    {
        ArrayList al = new ArrayList();
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach (string fileName in fileEntries)
        {
            int index = fileName.LastIndexOf("/");
            string localPath = "Assets/" + path;

            if (index > 0)
                localPath += fileName.Substring(index);

            Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

            if (t != null)
                al.Add(t);
        }

        T[] result = new T[al.Count];
        for (int i = 0; i < al.Count; i++)
            result[i] = (T) al[i];

        return result;
    }

    /// <summary>
    /// Get all subfolder names in a path
    /// </summary>
    /// <param name="path">Where you want to look at, can be relative to Assets or to project's folder</param>
    /// <returns></returns>
    public static IEnumerable<string> GetFolderNamesAtPath(string path)
    {
        path = RemoveAssetFromPath(path);
        path = Application.dataPath + "/" + path;
        string[] dirs = Directory.GetDirectories(path);
        return FixSeparatorsAndGetLastPathPart(dirs);
    }


    /// <summary>
    /// Get all file names in a path
    /// </summary>
    /// <param name="path">Where you want to look at, can be relative to Assets or to project's folder</param>
    /// <returns></returns>
    public static IEnumerable<string> GetFileNamesAtPath(string path)
    {
        path = RemoveAssetFromPath(path);
        path = Application.dataPath + "/" + path;
        string[] files = Directory.GetFiles(path);
        return FixSeparatorsAndGetLastPathPart(files);
    }

    private static IEnumerable<string> FixSeparatorsAndGetLastPathPart(IEnumerable<string> files)
    {
        return files.Select(filename =>
        {
            filename = filename.Replace("\\","/");
            int index = filename.LastIndexOf("/",StringComparison.Ordinal) + 1;
            return index > 0 ? filename.Substring(index) : filename;
        });
    }
    /// <summary>
    /// Get rid of a possible "Asset/" prefix
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string RemoveAssetFromPath(string path)
    {
        return path.StartsWith("Assets") ? path.Substring(7) : path;
    }

    /// <summary>
    /// just like Asset, but will first create the missing folders in the path if needed
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="path">Must be relative to Assets</param>
    public static void CreateFolderAndAsset(Object asset, string path)
    {
        string parent = Application.dataPath +"/"+Path.GetDirectoryName(RemoveAssetFromPath(path));
        parent = parent.Replace("\\", "/");
        if (!Directory.Exists(parent))
            Directory.CreateDirectory(parent);
        AssetDatabase.CreateAsset(asset, path);
    }


    public static void CreateFolderForAsset(string assetPath)
    {
        string parent = Application.dataPath + "/" + Path.GetDirectoryName(RemoveAssetFromPath(assetPath));
        parent = parent.Replace("\\", "/");
        if (!Directory.Exists(parent))
            Directory.CreateDirectory(parent);
    }
}
