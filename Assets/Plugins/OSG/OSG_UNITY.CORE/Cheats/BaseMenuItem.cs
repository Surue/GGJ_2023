// // Old Skull Games
// // Bernard Barthelemy
// // 14:18

using System.Collections.Generic;
using UnityEngine;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public abstract class BaseMenuItem
        {
            public static void MakeListLoop<T>(List<T> list) where T : BaseMenuItem
            {
                int count = list.Count;
                int prev = count - 1;
                for (int i = 0; i < count; ++i)
                {
                    int Next(int stride)
                    {
                        return (i + stride) % list.Count;
                    }
                    list[i].upItem = list[Next(prev)];
                    list[i].downItem = list[Next(1)];
                }
            }

            public abstract Rect CurrentPosition { get; }
            public BaseMenuItem upItem;
            public BaseMenuItem downItem;
            public BaseMenuItem rightItem;
            public BaseMenuItem leftItem;

            protected readonly BaseMenuItemSelector selector;

            public BaseMenuItem(BaseMenuItemSelector selector)
            {
                this.selector = selector;
            }

            public abstract bool OnGUI();
            public abstract void Click();
            public abstract void Focus();
        }
    }
}