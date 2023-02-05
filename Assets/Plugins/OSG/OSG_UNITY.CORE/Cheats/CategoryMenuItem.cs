// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020


using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public class CategoryMenuItem : ButtonMenuItem
        {
            public static CategoryMenuItem currentCategory;
            public List<CheatMenuItem> subItems;
            private GUIContent cacheContent;
            public CategoryMenuItem(BaseMenuItemSelector selector, CheatCategory category) : base(selector)
            {
                subItems = new List<CheatMenuItem>();
                foreach (CheatInfo cheatInfo in CheatSettings.CheatInfos)
                {
                    if (cheatInfo.category != category || !selector.manager.ShouldShowCheat(cheatInfo.method.Name))
                        continue;
                    subItems.Add(new CheatMenuItem(selector, cheatInfo, 
                        () => selector.ChangeSelectionTo(this)));
                }

                if (subItems.Any())
                {
                    int prevCount = subItems.Count - 1;
                    rightItem = subItems[0];
                    for (int i = 0; i < subItems.Count; ++i)
                    {
                        var item = subItems[i];
                        item.leftItem = this;
                        item.rightItem = item;
                        item.upItem = subItems[(i + prevCount) % subItems.Count];
                        item.downItem = subItems[(i + 1) % subItems.Count];
                    }

                    OnClick = () =>
                    {
                        selector.ChangeSelectionTo(this);
                        currentCategory = this;
                        selector.ChangeFocusTo(subItems[0]);
                    };

                    OnFocus = () =>
                    {
                        currentCategory = this;
                        selector.ChangeSelectionTo(null);
                    };

                    cacheContent = new GUIContent(category.ToString());
                    content = () => cacheContent;
                }
            }
        }
    }
}