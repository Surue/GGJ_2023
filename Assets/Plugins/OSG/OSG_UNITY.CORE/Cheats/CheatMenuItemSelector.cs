// Old Skull Games
// Bernard Barthelemy
// Thursday, July 6, 2017

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public class CheatMenuItemSelector : BaseMenuItemSelector
        {
            private GUIContent closeContent;
            private GUIContent plusContent;
            private GUIContent minusContent;

            public CheatMenuItemSelector(BaseCheatManager _manager) : base(_manager)
            {
                Array categories = Enum.GetValues(typeof(CheatCategory));

                mainMenuItems = new List<BaseMenuItem>(categories.Length);

                foreach (CheatCategory category in categories)
                {
                    if (manager.ShouldShowCategory(category))
                    {
                        var item = new CategoryMenuItem(this, category);
                        if (item.subItems != null && item.subItems.Any())
                        {
                            mainMenuItems.Add(item);
                        }
                    }
                }

                void OnFocusButton() => CategoryMenuItem.currentCategory = null;

                closeContent = new GUIContent("Close Menu".InColor("#cc0000"));
                mainMenuItems.Add(new ButtonMenuItem(this, () => closeContent,
                    () => manager.Show = false, OnFocusButton));
                plusContent = new GUIContent("+");
                mainMenuItems.Add(new ButtonMenuItem(this, ()=>plusContent, () => manager.UserRescaleMenu(0.05f),
                    OnFocusButton));
                minusContent = new GUIContent("-");
                mainMenuItems.Add(new ButtonMenuItem(this, () => minusContent, () => manager.UserRescaleMenu(-0.05f),
                    OnFocusButton));

                BaseMenuItem.MakeListLoop(mainMenuItems);
            }



            protected override void OnSubMenuGUI()
            {
                if (CategoryMenuItem.currentCategory != null)
                {
                    GUILayout.BeginVertical();
                    foreach (var item in CategoryMenuItem.currentCategory.subItems)
                    {
                        ItemGUI(item);
                    }

                    GUILayout.EndVertical();
                }

            }
        }
    }
}