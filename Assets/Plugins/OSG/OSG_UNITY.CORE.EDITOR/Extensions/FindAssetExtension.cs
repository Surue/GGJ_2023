// Old Skull Games
// Benoit Constantin
// Tuesday, February 05, 2019

using UnityEngine;
using UnityEditor;
using System.IO;

namespace OSG
{
    public static class FindAssetExtension
    {
        /// <summary>
        /// Get all instances of a type in the current project (and load it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetAllInstances<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        /// <summary>
        /// Get all instances of a type in the current project (and load it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetInstance<T>(string assetName) where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileName(path);

                if(fileName.Remove(fileName.IndexOf('.')) == assetName)
                    return  AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return null;
        }
    }
}