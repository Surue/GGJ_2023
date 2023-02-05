using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OSG
{
    public class ObjectData
    {
        public string name;
        public string objectPath;
        public string location;
        public bool inScene;
#if UNITY_2019_1_OR_NEWER
        public PrefabAssetType prefabType;
#else
        public PrefabType prefabType;
#endif

        [RenderData(0, "Name")]
        public virtual void RenderName(int width)
        {
            if (GUILayout.Button(new GUIContent(name, objectPath), GUILayout.Width(width)))
            {
                Locate();
            }
        }

        [SortData("Name")]
        public virtual void SortByName(List<ObjectData> list)
        {
            list.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
        }

        [RenderData(1, "Type")]
        public virtual void RenderPrefabType(int width)
        {

            GUILayout.Label(prefabType.ToString(), GUILayout.Width(width));
        }

        [SortData("Type")]
        public virtual void SortByPrefabType(List<ObjectData> list)
        {
            list.Sort((x, y) => (int) x.prefabType - (int) y.prefabType);
        }

        [RenderData(3, "Location")]
        public virtual void RenderScene(int width)
        {
            if (location != null)
            {
                GUILayout.Label(new GUIContent(location, location), GUILayout.Width(width));
            }
        }

        [SortData("Location")]
        public virtual void SortByScene(List<ObjectData> data)
        {
            data.Sort((x, y) => string.CompareOrdinal(x.location, y.location));
        }

        private void Locate()
        {
            if (inScene)
            {
                LocateInScene();
            }
            else
            {
                LocateInAssets();
            }
        }

        private void LocateInScene()
        {
            if (SceneManager.GetActiveScene().path != location)
            {
                EditorSceneManager.OpenScene(location);
                EditorApplication.update += PingLocation;
            }
            else
            {
                PingLocation();
            }
        }

        private void PingLocation()
        {
            var go = GameObject.Find(objectPath);
            if (go)
            {
                EditorGUIUtility.PingObject(go);
                Selection.activeGameObject = go;
                EditorApplication.update -= PingLocation;
            }
        }

        private void LocateInAssets()
        {
            Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(location);
            if (obj)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
        }
    }

    public class ObjectFinder<TData, TObject> where TData : ObjectData where TObject : UnityEngine.Object
    {
        class DataContainer : List<ObjectData>
        {
        }

        private static DataContainer _dataContainer;

        private static DataContainer Data => _dataContainer ?? (_dataContainer = new DataContainer());


        public static List<ObjectData> FindObjectInAllScenes(ObjectSelectorDelegate selector)
        {
            var scenes = AssetDatabase.FindAssets("t:Scene");
            Data.Clear();
            try
            {
                float total = scenes.Length;
                if(!typeof(ScriptableObject).IsAssignableFrom(typeof(TObject)))
                {
                    float current = 0;
                    foreach (string sceneGUID in scenes)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(sceneGUID);
                        ++current;
                        EditorUtility.DisplayProgressBar("Searching...", assetPath, current/total);
                        FindObjectsInScene(assetPath, selector);
                    }
                }

                FindObjectsInAssets(selector);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return Data;
        }

        private static void FindObjectsInAssets(ObjectSelectorDelegate selector)
        {
            var assetsPath = AssetDatabase.GetAllAssetPaths();
            float total = assetsPath.Length;
            float count = 0;

            foreach (string path in assetsPath)
            {
                ++count;
                EditorUtility.DisplayProgressBar("Searching in assets", path,count/total);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset)
                {
                    if (asset is TObject)
                    {
                        CheckAssetsObject(asset as TObject, selector);
                    }

                    if (asset is GameObject)
                    {
                        GameObject go = asset as GameObject;
                        MonoBehaviour[] allComponents = go.GetComponentsInChildren<MonoBehaviour>(true);
                        foreach (MonoBehaviour component in allComponents)
                        {
                            TObject o = component as TObject;
                            if (!o) continue;
                            CheckAssetsObject(o, selector);
                        }
                    }
                }
            }
        }


        public static List<ObjectData> FindAllObjectsInBuild(ObjectSelectorDelegate selector)
        {
            Data.Clear();
            float total = EditorBuildSettings.scenes.Length;
            try
            {
                if(!typeof(ScriptableObject).IsAssignableFrom(typeof(TObject)))
                {
                    float count = 0;
                    foreach (EditorBuildSettingsScene settingsScene in EditorBuildSettings.scenes)
                    {
                        ++count;
                        EditorUtility.DisplayProgressBar("Searching..", settingsScene.path, count/total);
                        FindObjectsInScene(settingsScene.path, selector);
                    }
                }
                FindObjectsInAssets(selector);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);      
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return Data;
        }

        public delegate TData ObjectSelectorDelegate(TObject obj);

        static void FindObjectsInScene(string scenePath, ObjectSelectorDelegate objectSelector)
        {
            EditorSceneManager.OpenScene(scenePath);
            TObject[] objs = Resources.FindObjectsOfTypeAll<TObject>();
            foreach (TObject obj in objs)
            {
                if(obj is ScriptableObject)
                    continue;

                if (obj )
                {
                    CheckSceneObject(scenePath, obj, objectSelector);
                }
            }
        }

        private static void CheckSceneObject(string scenePath, TObject obj, ObjectSelectorDelegate objectSelector)
        {
#if UNITY_2019_1_OR_NEWER
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(obj);
            if(prefabType != PrefabAssetType.NotAPrefab)
            {
                return;
            }

#else
            var prefabType = PrefabUtility.GetPrefabType(obj);
            if (prefabType == PrefabType.ModelPrefab || prefabType == PrefabType.Prefab)
            {
                return;
            }
#endif

            TData d = objectSelector(obj);
            if (d == null)
            {
                return;
            }

            d.name = obj.name;
        
            Component c = obj as Component;
            if (c)
            {
                d.objectPath = c.gameObject ? c.gameObject.HierarchyPath() : AssetDatabase.GetAssetPath(obj);
            }
            else
            {
                GameObject go = obj as GameObject;
                d.objectPath = go ? go.HierarchyPath() : "";
            }
            d.inScene = true;
            d.prefabType = prefabType;
            d.location = scenePath;
            Data.Add(d);
        }

        private static void CheckAssetsObject(TObject obj, ObjectSelectorDelegate objectSelector)
        {
#if UNITY_2019_1_OR_NEWER
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(obj);
            if (prefabType == PrefabAssetType.NotAPrefab)
            {
                return;
            }
              
#else
            var prefabType = PrefabUtility.GetPrefabType(obj);
            if (prefabType != PrefabType.ModelPrefab && prefabType != PrefabType.Prefab && prefabType != PrefabType.None)
            {
                return;
            }
#endif

            TData d = objectSelector(obj);
            if (d == null)
            {
                return;
            }

            d.name = obj.name;
            d.inScene = false;
            d.objectPath = d.location = AssetDatabase.GetAssetPath(obj);
            d.prefabType = prefabType;
            Data.Add(d);
        }

    }
}