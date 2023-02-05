using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using OSG.Core;

namespace OSG {
    public static class ScriptableObjectUtility
    {
        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
		public static T CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
			AddToDataBase(asset);
            return asset;
        }

		public static T CreateAssetAtPath<T>(string path, string nameWithoutExtension) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance(typeof(T)) as T;
            asset.name = nameWithoutExtension;
			AddToDataBase(asset, $"{path}/{nameWithoutExtension}.asset");
			return asset;
		}

        public static ScriptableObject CreateAssetAtPath(Type t, string path, string nameWithoutExtension)
        {
            var asset = ScriptableObject.CreateInstance(t);
            asset.name = nameWithoutExtension;
            AddToDataBase(asset, $"{path}/{nameWithoutExtension}.asset");
            return asset;
        }

        public static ScriptableObject CreateAsset(Type type)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AddToDataBase(asset);
            var methodInfo = type.GetMethod("OnSingletonCreate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (methodInfo != null)
            {
                methodInfo.Invoke(asset, null);
            }
            
            return asset;
        }

        private static void AddToDataBase(ScriptableObject asset, string assetPathAndName = "")
        {
            if (string.IsNullOrEmpty(assetPathAndName))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (path == "")
                {
                    path = "Assets";
                }
                else if (Path.GetExtension(path) != "")
                {
                    path = path.Replace(Path.GetFileName(path), "");
                }
                assetPathAndName =
                    AssetDatabase.GenerateUniqueAssetPath(path + "/New " + asset.GetType().Name + ".asset");
            }

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;


            Type assetType = asset.GetType();
            var methods = assetType.GetMethods();
            foreach (MethodInfo info in methods)
            {
                object[] attributes = info.GetCustomAttributes(typeof(OnCreateAttribute), true);
                if (attributes.Length > 0)
                {
                    try
                    {
                        info.Invoke(asset, new object[] {});
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                    }
                }
            }
        }

        public static T CreateAsset<T>(string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(path), "");
            }

            string assetPathAndName = string.Format("{0}/{1} {2}.asset", path, typeof(T).ToString(), name);
            AddToDataBase(asset, assetPathAndName);
            return asset;
        }


        private static List<Type> _scriptableTypes;

        public static List<Type> ScriptableTypes
        {
            get
            {
                if (_scriptableTypes == null)
                {
                    _scriptableTypes = new List<Type>();
                    AssemblyScanner.Register(ProcessType);
                    AssemblyScanner.Scan(() =>
                        {
                            _scriptableTypes.Sort((t1, t2) => string.CompareOrdinal(t1.FullName, t2.FullName));
                        });
                }
                return _scriptableTypes;
            }
        }

        public static void ProcessType(Type type)
        {
            if(type.IsAbstract)
                return;
            if (type.DerivesFrom(typeof(EditorWindow)))
                return;
            if (type.DerivesFrom(typeof(StateMachineBehaviour)))
                return;
            if (type.DerivesFrom(typeof(Editor)))
                return;
            if (type.DerivesFrom(typeof(ScriptableObject)))
            {
                ScriptableTypes.Add(type);
            }
        }
        
        public static void FocusInInspector (Type targetType) {
            if (targetType == null)
            {
                UnityEngine.Debug.LogError("Unknown type " + targetType);
                return;
            }
            string[] assets = AssetDatabase.FindAssets("t:" + targetType);
            List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
            for (var index = 0; index < assets.Length; ++index)
            {
                UnityEngine.Object o = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[index]), targetType);
                if (o)
                {
                    objs.Add(o);
                }
            }
            Selection.objects = objs.ToArray();
            if (Selection.activeObject)
            {
                Selection.activeObject.FocusInspector();
            }	
        }
    }
}