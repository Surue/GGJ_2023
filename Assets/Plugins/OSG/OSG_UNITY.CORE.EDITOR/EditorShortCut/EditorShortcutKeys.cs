using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    [Flags]
    public enum ShortcutType
    {
        FindSceneObject = 1,
        FindScriptableObject = 2,
        CallStaticMethod = 4
    }

    [Serializable]
    public class EditorShortcutKeys
    {
        [FieldSelector] public ShortcutType type;
        [SelectedField(1)] public string sceneObjectType;
        [SelectedField(2)] public string scriptableType;
        [SelectedField(4)] public SerializableMethodInfo  method;

        [SelectedField]public KeyCode code;
        [SelectedField]public Texture2D icon;

        public EditorShortcutKeys()
        {
            type = ShortcutType.FindSceneObject;
            code = KeyCode.None;
            icon = null;
            method = null;
            sceneObjectType = "Configure Me";
        }

        public Type TargetType
        {
            get
            {
                switch (type)
                {
                    case ShortcutType.FindSceneObject:
                        return Type.GetType(sceneObjectType);
                        
                    case ShortcutType.FindScriptableObject:
                        return Type.GetType(scriptableType);
                    case ShortcutType.CallStaticMethod:
                        return typeof(void);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override string ToString()
        {
            string result;
            switch (type)
            {
                case ShortcutType.FindSceneObject:
                    if(TargetType != null)
                        result = "Find " + TargetType.Name + " in scene";
                    else
                    {
                        result = "Find something in scene";
                    }
                    break;
                case ShortcutType.FindScriptableObject:
                    if(TargetType != null)
                        result = "Find " + TargetType.Name + " assets";
                    else
                    {
                        result = "Find some assets in project";
                    }
                    break;
                case ShortcutType.CallStaticMethod:
                    MethodInfo methodInfo = method==null ? null :  method.Info;
                    result = methodInfo == null
                        ? "Choose method"
                        : "Call " + methodInfo.DeclaringType.Name + "." + methodInfo.Name + "()";
                    break;
                default:
                    result = "Choose type";
                    break;
            }
            
            if(code != KeyCode.None)
                result += " (" + code +")";
            return result;
        }

        private GUIContent content;
        public GUIContent Content
        {
            get
            {
                return content ?? (content= new GUIContent(icon));
            }
        }

        public void Execute()
        {

            Type targetType;
            switch (type)
            {
                case ShortcutType.FindSceneObject:
                    targetType = TargetType;
                    if (targetType == null)
                    {
                        UnityEngine.Debug.LogError("Unknown type " + sceneObjectType);
                        return;
                    }
                    SceneModeUtility.SearchForType(targetType);

                    List<int> ids = new List<int>();
                    if(targetType.DerivesFrom(typeof(Component)))
                        ForEach.DoOnType(targetType, o => ids.Add((o as Component).gameObject.GetInstanceID()));
                    else
                        ForEach.DoOnType(targetType, o => ids.Add(o.GetInstanceID()));
                    Selection.instanceIDs = ids.ToArray();
                    if (Selection.activeObject)
                    {
                        Selection.activeObject.FocusInspector();
                    }
                    break;
                case ShortcutType.FindScriptableObject:
                    ScriptableObjectUtility.FocusInInspector(TargetType);
                    break;
                case ShortcutType.CallStaticMethod:
                    MethodInfo info = method.Info;
                    if (info == null)
                    {
                        UnityEngine.Debug.LogError("Unknown method");
                        return;
                    }
                    info.Invoke(null, new object[]{});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}