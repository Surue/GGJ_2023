// Old Skull Games
// Bernard Barthelemy
// Wednesday, August 1, 2018

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OSG.ConfigHelper
{
    [CustomEditor(typeof(TargetConfigHelper))]
    public class TargetConfigHelperEditor : Editor
    {
        private TargetConfigHelper helper;
        private void OnEnable()
        {
            helper = target as TargetConfigHelper;
            helper.SetElementsState(true);
            EditorApplication.projectWindowItemOnGUI += HierarchyOnGUI;
        }

        private void OnDisable()
        {
            EditorApplication.projectWindowItemOnGUI -= HierarchyOnGUI;
        }

        private void HierarchyOnGUI(string guid, Rect selectionrect)
        {
            if(Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                Debug.Log(guid);
                Event.current.Use();
            }
        }

        public static void DragDropAsset( Rect dropArea, Action<string> onDrop, float h = 25)
        {   
            // cache the current event
            Event currentEvent = Event.current;

            // if our mouse isn't contained within that box area, exit out
            if (!dropArea.Contains(currentEvent.mousePosition)) return;

            if (onDrop != null)
            {
                if (currentEvent.type == EventType.DragUpdated ||
                    currentEvent.type == EventType.DragPerform)
                {

                    // set the visual mode to copy
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    // if we dropped something
                    if (currentEvent.type == EventType.DragPerform)
                    {

                        // register that this drag-drop event has been handled by this control
                        DragAndDrop.AcceptDrag();
                        
                        foreach (string path in DragAndDrop.paths)
                        {
                            onDrop(path);
                        }
                    }

                    // since we've used the DragPerform event, we'll mark it as used
                    // (its type will change to EventType.Used)
                    // so that other controls ignore it
                    Event.current.Use();
                }
            }
        }

        private static Dictionary<BuildTargetGroup, Texture2D> targetIcons;

        public static Dictionary<BuildTargetGroup, Texture2D> TargetIcons
        {
            get
            {
                if (targetIcons == null)
                {
                    targetIcons = new Dictionary<BuildTargetGroup, Texture2D>();
                    Type buildPlaformsType = Type.GetType("UnityEditor.Build.BuildPlatforms,UnityEditor.dll");
                    
                    var instanceInfo = buildPlaformsType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
                    var instance = instanceInfo.GetValue(null);
                    var arrayInfo = buildPlaformsType.GetField("buildPlatforms");
                    object[] buildPlatforms = (object[])arrayInfo.GetValue(instance);

                    Type buildPlaformType = Type.GetType("UnityEditor.Build.BuildPlatform,UnityEditor.dll");
                    var targetGroupInfo = buildPlaformType.GetField("targetGroup");
                    var smallIconInfo = buildPlaformType.GetField("smallIcon");

                    foreach (object platform in buildPlatforms)
                    {
                        BuildTargetGroup target = (BuildTargetGroup) targetGroupInfo.GetValue(platform);
                        Texture2D icon = (Texture2D) smallIconInfo.GetValue(platform);
                        targetIcons.Add(target, icon);
                    }
                }

                return targetIcons;
            }
        }



        public override void OnInspectorGUI()
        {

            string message = helper.CheckErrors();
            if(!string.IsNullOrEmpty(message))
            {
                EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            // draw a box area for our drag-drop
            GUILayout.Space(15f);
            GUILayout.Label("");
            Rect top = GUILayoutUtility.GetLastRect();
            float headerSize = Math.Min(Screen.width * 0.5f, 250);
            DrawConfigs(headerSize);
            DrawElements(headerSize);
            GUILayout.Label("");
            Rect bottom = GUILayoutUtility.GetLastRect();
            Rect dropArea = new Rect(top.x, top.y, top.width, bottom.yMax - top.yMin);
            DragDropAsset(dropArea, s => { 
                Undo.RecordObject(helper, "Add " + s);
                helper.AddElement(s); 
                EditorUtility.SetDirty(helper);
            });

            if(GUILayout.Button("Generate .gitignore"))
            {
                helper.GenerateGitIgnore();
            }

            if(GUILayout.Button("Investigate Project Settings"))
            {
                helper.Investigate();
            }

            base.OnInspectorGUI();
        }


        private GUIStyle _chs;
        private GUIStyle ConfigHeaderStyle =>
            _chs ?? (_chs = new GUIStyle(GUI.skin.label)
            {
                richText = true
            });

        private void DrawConfigs(float headerSize)
        {
            if(!helper || helper.configs == null || helper.configs.Count == 0)
                return;

            GUILayout.Space(50);
            GUILayout.BeginHorizontal();
            GUILayout.Label(helper.elements.Count.ToString() + " elements ", GUILayout.MaxWidth(headerSize));
            
            GUIContent content = new GUIContent(" ");
            for (var index = 0; index < helper.configs.Count; index++)
            {
                var config = helper.configs[index];
                Color color = GUI.color;
                GUI.color = (EditorUserBuildSettings.selectedBuildTargetGroup == config.targetGroup 
                    ? Color.green
                    : Color.red)*0.25f + 0.75f * Color.white;
                if(GUILayout.Button(content, SmallButtonHeight, SmallButtonWidth))
                {
                    helper.ApplyConfig(config);
                    
                    AssetDatabase.Refresh();
                }

                Rect r = GUILayoutUtility.GetLastRect();

                Texture2D image;
                if (TargetIcons.TryGetValue(config.targetGroup, out image))
                {
                    r.width -= 2;
                    r.height -= 2;
                    r.x += 1;
                    r.y += 1;
                    GUI.DrawTexture(r, image);
                }

                Vector2 pos = r.center;

                Matrix4x4 m = GUI.matrix;
                GUIUtility.RotateAroundPivot(-45, pos);
                Rect rect = new Rect(pos.x+BUTTON_WIDTH*0.6f, pos.y-BUTTON_WIDTH*1.1f, 125, 125);
                GUI.Label(rect, config.name.InColor(helper.CurrentConfigName == config.name ? Color.green : Color.white),
                    ConfigHeaderStyle);
                GUI.matrix = m;
                GUI.color = color;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawElements(float headerSize)
        {
            //GUILayout.Label(helper.elements.Count.ToString() + " elements " );
            if (helper.elements == null)
            {
                return;
            }

            for (int index = helper.elements.Count; --index>=0;)
            {
                Element element = helper.elements[index];
                DrawElement(headerSize, element);
            }

            if(GUILayout.Button("Check all files"))
            {
                helper.CheckAllFiles();
            }
        }

        GUIContent OpenContent(string path)
        {
            return new GUIContent($"Open/{path.Remove(0, Application.dataPath.Length-6).Replace("/", @"\")}");
        }

        GUIContent CompareContent(string path)
        {
            return new GUIContent($"Compare Asset With/{path.Remove(0, Application.dataPath.Length - 6).Replace("/", @"\")}");
        }

        private void DrawElement(float headerSize, Element element)
        {
            GUILayout.BeginHorizontal();
            GUIContent content = new GUIContent((element.isFolder ? "Folder : " : "Asset : ") + element.name, element.AssetAbsolutePath);
            Color color = GUI.color;

            Config currentConfig = helper.CurrentConfig;
            bool? correct = currentConfig?.IsElementInCorrectState(element, false) == Element.ElementState.Ok;

            GUI.color = (correct.Value ? Color.green : Color.red)*0.25f+0.75f*Color.white;

            if(GUILayout.Button(content, GUI.skin.label, GUILayout.MaxWidth(headerSize)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(element.guid));
                if (asset)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }
            GUI.color = color;

            foreach (Config config in helper.configs)
            {
                var inclusion = config.Uses(element);

                GUIContent buttonContent;

                if(inclusion == null)
                {
                    buttonContent = new GUIContent("✘", "Not using");
                }
                else if(inclusion.overrideArchivePath)
                {
                    buttonContent = new GUIContent("o", "Using own version");
                }
                else
                {
                    buttonContent = new GUIContent("✓", "Using common version");
                }

                if (GUILayout.Button(buttonContent, SmallButtonHeight, SmallButtonWidth))
                {
                    GenericMenu menu = new GenericMenu();
                    var c = config;
                    menu.AddItem(new GUIContent("Use Common Archive (✓)"), false, () => { 
                        Undo.RecordObject(helper, element.name + " usage ");
                        c.UseCommon(element);
                        EditorUtility.SetDirty(helper);
                    });
                    menu.AddItem(new GUIContent("Use " + c.name + " Override (o)"), false, () => {
                        Undo.RecordObject(helper, element.name + " usage ");
                        c.UseOwn(element); 
                        EditorUtility.SetDirty(helper);
                    });
                    menu.AddItem(new GUIContent("Don't Use (✘)" ), false, () => {
                        Undo.RecordObject(helper, element.name + " usage ");
                        c.Remove(element);
                        EditorUtility.SetDirty(helper);
                    });

                    menu.AddSeparator("");

                    if(element.Exists(element.AssetAbsolutePath))
                    {
                        menu.AddItem(OpenContent(element.AssetAbsolutePath), false, func: () => System.Diagnostics.Process.Start(element.AssetAbsolutePath));
                    }
                    string archivePath = config.GetArchiveAbsolutePath(element, true);
                    if(element.Exists(archivePath))
                    {
                        menu.AddItem(OpenContent(archivePath), false, () => System.Diagnostics.Process.Start(archivePath));
                        menu.AddItem(CompareContent(archivePath), false,
                            () => TargetConfigHelper.Diff(element.AssetAbsolutePath, archivePath));
                    }
                    archivePath = config.GetArchiveAbsolutePath(element, false);
                    if (element.Exists(archivePath))
                    {
                        menu.AddItem(OpenContent(archivePath), false,
                            () => System.Diagnostics.Process.Start(archivePath));
                        menu.AddItem(CompareContent(archivePath), false,
                            () => TargetConfigHelper.Diff(element.AssetAbsolutePath, archivePath));
                    }

                    menu.ShowAsContext();
                }
            }

            if(GUILayout.Button("☰", SmallButtonWidth))
            {
                GenericMenu menu = new GenericMenu();
                if (currentConfig != null)
                {
                    menu.AddItem(new GUIContent("Check Status"), false, () => {
                        element.GetStateForConfig(currentConfig, true);    
                    });
                    
                    Inclusion inclusion = currentConfig.Uses(element);
                    if(inclusion != null)
                    {
                        string archiveAbsolutePath = currentConfig.GetArchiveAbsolutePath(element, inclusion.overrideArchivePath);
                        string displayPath = archiveAbsolutePath.Remove(0,Config.ArchiveRoot.Length).Replace("/",@"\");
                        if (element.Exists(element.AssetAbsolutePath))
                        {
                            menu.AddItem(new GUIContent($"Push to {displayPath}"), false, () => { 
                                element.Copy(element.AssetAbsolutePath, archiveAbsolutePath);
                                element.GetStateForConfig(currentConfig, true);
                            });
                        }

                        if (element.Exists(archiveAbsolutePath))
                        {
                            menu.AddItem(new GUIContent($"Pull from {displayPath}"), false, () => { 
                                element.Copy(archiveAbsolutePath, element.AssetAbsolutePath);
                                element.GetStateForConfig(currentConfig, true);
                            });
                        }
                    }
                }

                menu.AddItem(new GUIContent("Remove from elements"), false, () => {
                    Undo.RecordObject(helper, "Remove " + element.name);
                    helper.RemoveElement(element);
                    EditorUtility.SetDirty(helper);
                });


                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();
            
        }


        private GUILayoutOption SmallButtonWidth = GUILayout.Width(BUTTON_WIDTH);
        private GUILayoutOption SmallButtonHeight = GUILayout.Height(BUTTON_WIDTH);
        public const float BUTTON_WIDTH = 20;

   
    }
}