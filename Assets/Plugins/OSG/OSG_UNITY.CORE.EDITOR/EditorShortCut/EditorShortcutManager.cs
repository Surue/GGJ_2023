using System;
using System.Collections.Generic;
using System.Reflection;
using OSG.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OSG
{
    [InitializeOnLoad]
    public static class EditorShortcutManager
    {
        [EditorPrefs] public static float iconSize = 24;
        [EditorPrefs] public static Vector2 position = new Vector2(8,24);
        
        [EditorPrefs] public static TextAnchor anchor = TextAnchor.UpperRight;
        [EditorPrefs] public static Slider.Direction direction = Slider.Direction.TopToBottom;
        [EditorPrefs] public static string buttonSyle = "Button";

        public static BindingFlags MethodsBindingFlags =
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        public class SceneObjectClassesContainer : List<Type>{}
        public class ScriptableObjectClassesContainer : List<Type>{}
        public class CallableMethodsContainer : List<MethodInfo>{}

        private static SceneObjectClassesContainer sceneObjects;
        public static SceneObjectClassesContainer SceneObjects{get { return sceneObjects ?? (sceneObjects = new SceneObjectClassesContainer()); }}

        private static ScriptableObjectClassesContainer scriptableObjects;
        public static ScriptableObjectClassesContainer ScriptableObjects{get { return scriptableObjects ?? (scriptableObjects = new ScriptableObjectClassesContainer()); }}

        private static CallableMethodsContainer methods;
        public static CallableMethodsContainer Methods{get { return methods ?? (methods = new CallableMethodsContainer()); }}

        private static EditorShortcutList[] shortcutLists;

        static EditorShortcutManager()
        {
            EditorListener.OnNextUpdateDo(Init);
        }

        public static string GetContent(MethodInfo m)
        {
            return m.DeclaringType.Name[0] + "/" + m.DeclaringType.Name + "/" + m.Name;
        }


        private static void Init()
        {
            EditorListener.OnNextUpdateDo(() =>
            {
#if UNITY_2019_1_OR_NEWER
                SceneView.duringSceneGui += OnSceneGUI;
#else
                SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
                AssemblyScanner.Scan(() =>
                {
                    Comparison<Type> comparison = (t1, t2) => string.CompareOrdinal(t1.Name, t2.Name);
                    ScriptableObjects.Sort(comparison);
                    SceneObjects.Sort(comparison);
                    Methods.Sort((m1, m2) => string.CompareOrdinal(GetContent(m1), GetContent(m2)));
                    shortcutLists = EditorShortcutList.Instances.ToArray();
                });
            });
        }

        private static bool FilterAssemblyOut(Assembly assembly)
        {
            return !assembly.FullName.StartsWith("Assembly-CSharp")
                && !assembly.FullName.Contains("UnityEngine");
        }

        [AssemblyScanner.ProcessType("FilterAssemblyOut")]
        public static void ProcessType(Type type)
        {
            if (type.DerivesFrom(typeof(Component)))
            {
                SceneObjects.Add(type);
            }
            else if (type.DerivesFrom(typeof(ScriptableObject))
                 && !type.DerivesFrom(typeof(EditorWindow)))
            {
                ScriptableObjects.Add(type);
            }

            var typesMethods = type.GetMethods( MethodsBindingFlags );
            foreach (MethodInfo info in typesMethods)
            {
                if(info.IsGenericMethod) continue;
                if(info.ContainsGenericParameters) continue;
                if(info.IsConstructor) continue;
                if(info.MemberType == MemberTypes.Property) continue;
                if(info.GetParameters().Length>0)continue;
                Methods.Add(info);
            }

        }

        private static void OnSceneGUI(SceneView sceneview)
        {
            if (shortcutLists != null)
            {
                Rect screenRect = sceneview.camera.pixelRect;

                float x,y;
                Handles.BeginGUI();
                switch (anchor)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        x = position.x;
                        break;
                    case TextAnchor.UpperCenter:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.LowerCenter:
                        x = screenRect.width*0.5f;
                        break;
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        x = screenRect.width - (iconSize + position.x + 4);
                        break;
                        default: return;
                }

                switch (anchor)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        y = position.y;
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.MiddleRight:
                        y = screenRect.height * 0.5f + position.y;
                        break;
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        y = screenRect.height - (position.y + iconSize + 40);
                        break;
                        default: return;
                }

                Vector2 directionVector;
                switch (direction)
                {
                    case Slider.Direction.LeftToRight:
                        directionVector = new Vector2(1,0);
                        break;
                    case Slider.Direction.RightToLeft:
                        directionVector = new Vector2(-1,0);
                        break;
                    case Slider.Direction.BottomToTop:
                        directionVector = new Vector2(0, -1);
                        break;
                    case Slider.Direction.TopToBottom:
                        directionVector = new Vector2(0,1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                GUIStyle style = buttonSyle;
                Rect rect = new Rect(x, y, iconSize, iconSize);
                for (int i = 0; i < shortcutLists.Length; ++i)
                {
                    var list = shortcutLists[i];
                    list.UpdateGUI(ref rect, directionVector, style);
                }

                Handles.EndGUI();
            }
        }


        public static GenericMenu GetSceneObjectsMenu(Action<Type> doAction)
        {
            return GetMenuFor(SceneObjects, doAction);
        }

        private static GenericMenu GetMenuFor(IEnumerable<Type> types, Action<Type> action)
        {
            GenericMenu menu = new GenericMenu();
            foreach (Type type in types)
            {
                GUIContent content = new GUIContent(type.Name[0].ToString() + "/" + type.Name);
                Type t1 = type;
                menu.AddItem(content, false, () => action(t1));
            }
            return menu;
        }

        public static GenericMenu GetScriptablesMenu(Action<Type> action)
        {
            return GetMenuFor(ScriptableObjects, action);
        }

        public static void OpenAnimator()
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
#else
            EditorApplication.ExecuteMenuItem("Window/Animator");
#endif
        }

    }
}