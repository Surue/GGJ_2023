// Old Skull Games
// Bernard Barthelemy
// Tuesday, July 4, 2017

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OSG
{
    public class CheatInfo
    {
        public readonly MethodInfo method;
        private readonly MethodInfo GetContentMethod;
        private readonly CheatShortcut cheatShortcut;
        private readonly string shortcut;
        private readonly string label;
        private readonly GUIContent content;
        public readonly CheatCategory category;
        public readonly bool autoCloseMenu;

        private CheatInfo(MethodInfo mI, CheatFunctionAttribute attribute, Type type)
        {
            autoCloseMenu = attribute.autoClose;
            method = mI;
            cheatShortcut = CheatSettings.HasAsset ? CheatSettings.Instance.GetCheatShortcutForMethod(method.Name) : null;
            if (cheatShortcut != null)
            {
                cheatShortcut.cheatInfo = this;
            }

            if (!string.IsNullOrEmpty(attribute.CustomMenuDisplay))
            {
                GetContentMethod = type.GetMethod(attribute.CustomMenuDisplay,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (GetContentMethod != null)
                {
                    if (GetContentMethod.ReturnType != typeof(string)
                     || GetContentMethod.GetParameters().Length > 0)
                    {
                        GetContentMethod = null;
                    }
                }
            }
            
            label = method.Name;
            shortcut = cheatShortcut == null || SystemInfo.deviceType == DeviceType.Handheld
                ? string.Empty
                : cheatShortcut.Label().InSize(8).InColor(Color.blue);

            content = new GUIContent(label.BeautifyName() + shortcut);
            category = attribute.category;
        }

        public string ToString(IEnumerable<object> p)
        {
            string text = method.Name + "(";
            if (p != null)
            {
                bool first = true;
                foreach (var o in p)
                {
                    if (first)
                        first = false;
                    else
                        text += ", ";
                    text += o == null ? "null" : o.ToString();
                }
            }
            text += ")";
            return text;            
        }
        
        public override string ToString()
        {
            return ToString(null);
        }

        public void Execute(object[] p)
        {
            CheatMessages.Display(ToString(p));
            method.Invoke(null, p);
        }

        public static List<CheatInfo> GetCheatInfos(Type type)
        {
            List<CheatInfo> result = null;
            MethodInfo[] methodInfos =
                type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo info in methodInfos)
            {
                object[] attributes = info.GetCustomAttributes(typeof(CheatFunctionAttribute), false);
                if (attributes.Any())
                {
                    CheatFunctionAttribute att = attributes[0] as CheatFunctionAttribute;
                    result = result ?? new List<CheatInfo>();
                    result.Add(new CheatInfo(info, att, type));
                }
            }

            return result;
        }
        
        public GUIContent GetContent()
        {
            if (GetContentMethod != null)
            {
                content.text = GetContentMethod.Invoke(null,null) + shortcut;
            }
            return content;
        }
    }
}

