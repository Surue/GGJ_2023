// Old Skull Games
// Bernard Barthelemy
// Tuesday, September 3, 2019

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OSG
{
    static class FindNotLocalizedAssets 
    {
        [EditorPrefs] private static bool verbose = false;
        [EditorPrefs] private static bool prefabs = false;
        [EditorPrefs] private static bool assets = true;
        [EditorPrefs] private static bool scenes = false;

        private const string lookString = "Looking for not localized stuff in";

        [MenuItem("OSG/Localization/Find not localized assets")]
        private static void FindEmAll()
        {
            string[] paths = AssetDatabase.GetAllAssetPaths();
            EditorUtility.ClearProgressBar();
            try
            {
                if (!Localization.HasAnyKey)
                {
                    var localization = Localization.Instance;
                    localization.LoadLanguage(localization.CurrentLanguage);
                }

                seen.Clear();

                if (prefabs || assets)
                {
                    
                    string title = lookString;
                    if(prefabs)
                    {
                        title += " prefabs";
                    }
                    if(assets)
                    {
                        if (prefabs)
                            title += " and ";
                        title += "assets";
                    }

                    for (var index = 0; index < paths.Length; index++)
                    {
                        string path = paths[index];
                        if (EditorUtility.DisplayCancelableProgressBar(title, path,
                            (float) index / paths.Length))
                            break;

                        if (assets && path.EndsWith(".asset"))
                        {
                            FindInAsset(path);
                        }

                        if (prefabs && path.EndsWith(".prefab"))
                        {
                            FindInPrefab(path);
                        }
                    }
                }

                if (scenes)
                {
                    FindInScenes();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            Debug.Log($"{seen.Count} objects checked");
            seen.Clear();
            EditorUtility.ClearProgressBar();
        }

        private static void FindInScenes()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string title = $"{lookString} scenes";
                EditorBuildSettingsScene[] paths = EditorBuildSettings.scenes;
                for (var index = 0; index < paths.Length; index++)
                {
                    string scenePath = paths[index].path;

                    if (EditorUtility.DisplayCancelableProgressBar(title, scenePath,
                        (float) index / paths.Length))
                        break;

                    if (verbose)
                    {
                        Debug.Log($"Looking in scene {scenePath}");
                    }

                    Scene openScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    foreach (var gameObject in openScene.GetRootGameObjects())
                    {
                        FindInGameObject($"{scenePath}::{gameObject.name}", gameObject);
                    }
                }
            }
        }

        private static void FindInPrefab(string path)
        {
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!mainAsset)
            {
                Debug.LogWarning($"{path} asset is null");
                return;
            }

            GameObject go = mainAsset as GameObject;
            if(!go)
            {
                Debug.LogWarning($"{path} has no game object", mainAsset);
                return;
            }
            FindInGameObject(path, go);
        }

        private static void FindInGameObject(string path, GameObject go)
        {
            if(verbose)
            {
                Debug.Log($"Looking in {go.name}");
            }

            Component[] components = go.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (!component)
                {
                    Debug.LogWarning($"{path} is missing a component");
                    return;
                }

                FindInObject(component, component, $"{path}:{component.GetType().Name} {component.name}", 0);
            }
        }

        private static void FindInAsset(string path)
        {
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!mainAsset)
                return;
            FindInObject(mainAsset, mainAsset, path,0);
        }


        private static HashSet<object> seen = new HashSet<object>();


        private static BindingFlags flags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public;
        private static void FindInObject(Object asset, object member, string path, int depth)
        {
            if (member == null)
            {
                return;
            }

            Type type = member.GetType();
            if(verbose)
                Debug.Log($"Looking at {path} {type.AssemblyQualifiedName}");

            if (type == typeof(Localization))
            {
                return;
            }

            if (++depth > 1000)
            {
                throw new Exception($"{path} is too deep... smelly!");
            }

            if (type.IsClass)
            {
                if (seen.Contains(member))
                {
                    return;
                }

                seen.Add(member);
            }

            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                Type fieldType = field.FieldType;

                if(fieldType == typeof(void))
                    continue;

                if (fieldType.IsPrimitive)
                {
                    continue;
                }

                if (fieldType == typeof(string))
                {
                    if (HasLocalizedTextAttribute(field))
                        Check(asset, member, path, field);
                    continue;
                }


                var value = field.GetValue(member);
                if (value != null)
                {
                    var l = value as IList;
                    if (l != null && l.Count > 0)
                    {
                        Type elementType = fieldType.GetElementType();
                        if(elementType == null)
                        {
                            Type[] fieldTypeGenericTypeArguments = fieldType.GenericTypeArguments;
                            if(fieldTypeGenericTypeArguments.Length>0)
                            {
                                elementType = fieldTypeGenericTypeArguments[0];
                                if (elementType == null)
                                {
                                    Debug.LogError($"{path} {fieldType.Name} is a list without ElementType");
                                    continue;
                                }
                            }
                            // dunno what this is, let it go through, to be safe
                        }

                        if(elementType != null && elementType.IsPrimitive)
                        {
                            Debug.Log($"{path} {fieldType} {fieldType.Name} is a primitive type, skipped");
                            continue;
                        }

                        var isLocalizedTextArray = elementType == typeof(string) && HasLocalizedTextAttribute(field);

                        for (var index = 0; index < l.Count; index++)
                        {
                            string newPath = $"{path}.{field.Name}[{index}]";
                            var o = l[index];
                            if (o != null)
                            {
                                if (isLocalizedTextArray)
                                {
                                    string ss = o as string;
                                    if (ss != null)
                                    {
                                        CheckString(asset, newPath, ss);
                                    }
                                    else
                                    {
                                        Debug.LogError($"{newPath} is not a string");
                                    }
                                }
                                else
                                {
                                    FindInObject(asset, o, newPath, depth);
                                }
                            }
                            else if(isLocalizedTextArray)
                            {
                                Debug.LogError($"{newPath} is null");
                            }
                        }

                        continue;
                    }
                    FindInObject(asset, value, $"{path}.{field.Name}", depth);
                }
            }
        }

        private static bool HasLocalizedTextAttribute(FieldInfo field)
        {
            return field.GetCustomAttributes(true).FirstOrDefault(attribute => attribute is LocalizedTextAttribute) != null;
        }

        private static void Check(Object asset, object member, string path, FieldInfo field)
        {
            string value = field.GetValue(member) as string;
            if (value != null)
            {
                CheckString(asset, $"{path}.{field.Name}", value);
            }
            else
            {
                Debug.LogWarning($"{path} : has no string value");
            }
        }

        private static void CheckString(Object asset, string path, string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                Debug.LogWarning($"{path} = is empty");
            }
            else if (!Localization.HasKey(value))
            {
                Debug.LogError($"{path} = {value} is not localized", asset);
            }
            else if(verbose)
            {
                Debug.Log($"{path} : {value} =>{Localization.Localize(value)}");
            }
        }
    }
}
