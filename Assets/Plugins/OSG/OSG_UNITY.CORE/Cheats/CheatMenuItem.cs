// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020


using System;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public class CheatMenuItem : ButtonMenuItem
        {
            public CheatMenuItem(BaseMenuItemSelector selector, CheatInfo cheatInfo, Action onFocus) : base(selector,
                cheatInfo.GetContent, () =>
                {
                    if (cheatInfo.autoCloseMenu)
                    {
                        selector.manager.Show = false;
                    }
                    selector.manager.Call(cheatInfo);
                }, onFocus)
            {
            }
        }
    }
}