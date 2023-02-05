// Old Skull Games
// Bernard Barthelemy
// Tuesday, July 4, 2017

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OSG.Core;
using UnityEngine;

namespace OSG
{
    public enum CheatCategory
    {
        Time,
        Global,
        InGame,
        Perf,
        Data
    }

    public class CheatSettings : ScriptableObjectSingleton<CheatSettings>
    {
        [SerializeField] public CheatShortcut[] cheatShortcuts;
        [SerializeField] public CheatMethodName[] cheatsToHide;
        [SerializeField] public Vector2 menuItemSizeInInch = new Vector2(3, 1);
        [SerializeField] public Vector2 menuItemPosInInch = new Vector2(150f, 0f);
        [SerializeField] public GUISkin guiSkin;

        public static void Setup()
        {
            _cheatInfos = new List<CheatInfo>();
            AssemblyScanner.Register(ProcessType);
            AssemblyScanner.Scan(() => {
                for (var index = 0; index < _cheatInfos.Count; index++)
                {
                    string name = _cheatInfos[index].method.Name;
                    for (var nextIndex = index + 1; nextIndex < _cheatInfos.Count; ++nextIndex)
                    {
                        if (_cheatInfos[nextIndex].method.Name == name)
                        {
                            UnityEngine.Debug.LogError("Duplicated cheat named " + name + " in classes " +
                                                       _cheatInfos[index].method.DeclaringType + " and " +
                                                       _cheatInfos[nextIndex].method.DeclaringType + " and ");
                        }
                    }
                }
                _cheatInfos?.Sort((c0, c1) => string.CompareOrdinal(c0.method.Name, c1.method.Name));

            });
        }
        
        private static List<CheatInfo> _cheatInfos;
        public static List<CheatInfo> CheatInfos
        {
            get
            {
                if (_cheatInfos == null)
                {
                    Setup();
                }
                return _cheatInfos;
            }
        }

        private static void ProcessType(Type type)
        {
            List<CheatInfo> addThose = CheatInfo.GetCheatInfos(type);
            if (addThose != null)
            {
                CheatInfos.AddRange(addThose);
            }
        }

        public CheatShortcut GetCheatShortcutForMethod(string methodName)
        {
            return cheatShortcuts == null
                ? null
                : cheatShortcuts.FirstOrDefault(cheatShortcut => methodName == cheatShortcut.name);
        }
    }
}
