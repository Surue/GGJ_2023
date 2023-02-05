// Old Skull Games
// Bernard Barthelemy
// Wednesday, August 1, 2018

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace OSG.ConfigHelper
{
    
    class TargetConfigHelper : ScriptableObject
    {
        [SerializeField] public string commonDefines;
        [SerializeField] [HideInInspector] public List<Element> elements;
        [SerializeField] public List<Config> configs;

        [FormerlySerializedAs("currentConfig")] [SerializeField] [HideInInspector] private string currentConfigName;

        public string CurrentConfigName => currentConfigName;

        private Config curConfig;

        public Config CurrentConfig
        {
            set
            {
                currentConfigName = value?.name;
                curConfig = value;
            }
            get { 
                if(curConfig==null)
                {
                    if (!string.IsNullOrEmpty(currentConfigName))
                    {
                        curConfig = configs.FirstOrDefault(config => config.name == currentConfigName);
                        if(curConfig == null)
                            currentConfigName = "";
                    }

                    return curConfig;
                }

                if(curConfig.name != CurrentConfigName)
                {
                    curConfig = null;
                    return CurrentConfig;
                }

                return curConfig;
            }
        }


        public void ApplyConfig(Config config)
        {
            config?.Apply(this);
            CurrentConfig = config;
        }

        public void ApplyConfig(string configName)
        {
            ApplyConfig(configs.First(config => config.name == configName));
        }

        public void AddElement(string path)
        {
            Element alreadyExists=null;
            foreach (Element element in elements)
            {
                if(element.assetRelativePath == path)
                {
                    alreadyExists = element;
                    break;
                }
            }
            if(alreadyExists==null)
            {
                alreadyExists= new Element(path);
                elements.Add(alreadyExists);
            }
            else
            {
                foreach (var config in configs)
                {
                    config.UpdateArchive(alreadyExists);
                }
            }

            CreateCommonArchive(alreadyExists);
            EditorUtility.SetDirty(this);
            GenerateGitIgnore();
        }

        private void CreateCommonArchive(Element element)
        {
            string archivePath = DirectoryHelpers.Combine(Config.ArchiveRoot, Config.common, element.assetRelativePath);
            element.Copy(element.AssetAbsolutePath, archivePath);
        }

        public void RemoveElement(Element element)
        {
            elements.Remove(element);
            foreach (Config config in configs)
            {
                config.Remove(element);
            }
            GenerateGitIgnore();
        }

        public void GenerateGitIgnore()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Element element in elements)
            {
                sb.AppendLine($"{element.assetRelativePath}{(element.isFolder ? "/" : "")}");
                sb.AppendLine($"{element.assetRelativePath}.meta");
            }
            File.WriteAllText($"{Application.dataPath}/.gitignore", sb.ToString());
        }

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            var configHelpers = Resources.FindObjectsOfTypeAll<TargetConfigHelper>();
            if (configHelpers.Length == 0)
            {
                //Debug.Log("No config helper detected");
                return;
            }

            if(configHelpers.Length > 1)
            {
                Debug.LogError($"{configHelpers.Length} config helper detected! the first one will be used");
            }

            var helper = configHelpers[0];
            helper.OnLoadInternal();
        }

        private void OnLoadInternal()
        {
            if(!string.IsNullOrEmpty(CheckErrors()))
            {
                Selection.activeObject = this;
            }
        }

        public string CheckErrors()
        {
            if (configs == null || configs.Count == 0)
                return null;


            bool currentConfigExists = false;
            foreach (Config config in configs)
            {
                if(config.name == CurrentConfigName)
                {
                    currentConfigExists = true;
                    break;
                }
            }

            if(!currentConfigExists)
            {
                currentConfigName = null;
            }

            if(string.IsNullOrEmpty(CurrentConfigName))
            {
                return "No current configuration selected, please select one";
            }

            return null;
        }

        public void SetElementsState(bool forceUpdate )
        {
            Config cur = CurrentConfig;
            foreach (Element element in elements)
            {
                element.GetStateForConfig(cur, forceUpdate);
            }
        }

        public void CheckAllFiles()
        {
            SetElementsState(true);
            var actions = CheckUnusedFiles();
            foreach (Action action in actions)
            {
                action?.Invoke();
            }
        }

        private static Action ActionMessage(string message)
        {
            return () => EditorUtility.DisplayDialog("", message, "ok");
        }

        private static Action ActionDeleteFile(string message, string path, Element element)
        {
            return () =>
            {
                if (EditorUtility.DisplayDialog("", message, "yes", "no"))
                {
                    element.Delete(path);
                }
            };
        }

        private Action ActionRemoveElement(string message, Element element)
        {
            return () => { 
                if(EditorUtility.DisplayDialog("", message, "yes", "no"))
                {
                    RemoveElement(element);
                }
            };
        }


        private List<Action> CheckUnusedFiles()
        {
            List<Action> proposeActions = new List<Action>();
            if(configs==null || configs.Count == 0)
            {
                proposeActions.Add(ActionMessage("No config"));
            }
            else
            {
                if(elements == null || elements.Count == 0)
                {
                    proposeActions.Add(ActionMessage("No elements defined. You can drag and drop an asset in the inspector to add it as element"));
                }
                else
                {
                    foreach (Element element in elements)
                    {
                        bool usedInCommon = false;
                        bool usedInOverride = false;

                        
                        foreach (Config config in configs)
                        {
                            Inclusion inclusion = config.Uses(element);
                            if (inclusion == null)
                            {
                                continue;
                            }
                        
                            usedInCommon = usedInCommon || !inclusion.overrideArchivePath;
                            usedInOverride = usedInOverride || inclusion.overrideArchivePath;

                            string archiveAbsolutePath = config.GetArchiveAbsolutePath(element,true);
                            if(!inclusion.overrideArchivePath && element.Exists(archiveAbsolutePath))
                            {
                                proposeActions.Add(ActionDeleteFile(
                                    $"File {archiveAbsolutePath} is not used, delete it?", archiveAbsolutePath,
                                    element));
                            }
                        }

                        if(!(usedInCommon || usedInOverride))
                        {
                            proposeActions.Add(ActionRemoveElement($"Element {element.name} is never used, Remove it?", element));
                        }
                        else if(!usedInCommon)
                        {
                            string archiveAbsolutePath = configs[0].GetArchiveAbsolutePath(element, false);
                            if(!string.IsNullOrEmpty(archiveAbsolutePath) && element.Exists(archiveAbsolutePath))
                            {
                                proposeActions.Add(ActionDeleteFile($"File {archiveAbsolutePath} is never used, delete it?", archiveAbsolutePath, element));
                            }
                        }
                    }
                }
            }

            return proposeActions;
        }

        [EditorPrefs] private static string diffPath = @"C:\Program Files\KDiff3\kdiff3.exe";
        [EditorPrefs] private static string diffParameters = " {0} {1}";

        public static void Diff(string path1, string path2)
        {
            System.Diagnostics.Process.Start(diffPath, string.Format(diffParameters, path1, path2));
        }


        public void Investigate()
        {
            MethodInfo mInfo = typeof(PlayerSettings).GetMethod("GetSerializedObject", BindingFlags.Static | BindingFlags.NonPublic);

            if(mInfo != null)
            {
                object value = mInfo.Invoke(null, new object[] { });
                SerializedObject so = value as SerializedObject;
                var prop = so?.GetIterator();
                if (prop != null)
                {
                    prop.Next(true);
                    do
                    {
                        Debug.Log(prop.ValueText().InMagenta());
                    } while (prop.Next(false));
                }
            }

        }
    }


    public static class SerializedPropertyExtension
    {

        public static object GetValueAsBaseObject(this SerializedProperty prop)
        {
            SerializedPropertyType type;

            if (Enum.TryParse(prop.type, out type))
            {
                switch (type)
                {
                    case SerializedPropertyType.Generic:
                        return prop.objectReferenceValue;
                    case SerializedPropertyType.Integer:
                        return prop.intValue;
                    case SerializedPropertyType.Boolean:
                        return prop.boolValue;
                    case SerializedPropertyType.Float:
                        return prop.floatValue;
                    case SerializedPropertyType.String:
                        return prop.stringValue;
                    case SerializedPropertyType.Color:
                        return prop.colorValue;
                    case SerializedPropertyType.ObjectReference:
                        return prop.objectReferenceInstanceIDValue;                        
                    case SerializedPropertyType.LayerMask:
                        return prop.intValue;
                    case SerializedPropertyType.Enum:
                        return prop.enumNames[prop.enumValueIndex];
                    case SerializedPropertyType.Vector2:
                        return prop.vector2Value;
                    case SerializedPropertyType.Vector3:
                        return prop.vector3Value;
                    case SerializedPropertyType.Vector4:
                        return prop.vector4Value;
                    case SerializedPropertyType.Rect:
                        return prop.rectValue;
                    case SerializedPropertyType.ArraySize:
                        break;
                    case SerializedPropertyType.Character:
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        break;
                    case SerializedPropertyType.Bounds:
                        break;
                    case SerializedPropertyType.Gradient:
                        break;
                    case SerializedPropertyType.Quaternion:
                        break;
                    case SerializedPropertyType.ExposedReference:
                        break;
                    case SerializedPropertyType.FixedBufferSize:
                        break;
                    case SerializedPropertyType.Vector2Int:
                        break;
                    case SerializedPropertyType.Vector3Int:
                        break;
                    case SerializedPropertyType.RectInt:
                        break;
                    case SerializedPropertyType.BoundsInt:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        public static string ValueText(this SerializedProperty prop)
        {
            object o = prop.GetValueAsBaseObject();
            return $"{prop.type.InColor(Color.cyan)} {prop.name} {(o==null?"null": o.ToString()).InColor(Color.yellow)}";
        }
    }




}
