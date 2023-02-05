// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020


using System;
using UnityEngine;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public class ButtonMenuItem : BaseMenuItem
        {
            protected Func<GUIContent> content;
            protected Action OnClick;
            protected Action OnFocus;
            protected Rect currentRect;

            public ButtonMenuItem(BaseMenuItemSelector selector, Func<GUIContent> content, Action onClick, Action onFocus) : base(selector)
            {
                this.content = content;
                OnClick = onClick;
                OnFocus = onFocus;
            }

            public ButtonMenuItem(BaseMenuItemSelector selector, Func<GUIContent> content, Action onClick) : this(selector, content, onClick, DoNothing)
            {
            }

            protected ButtonMenuItem(BaseMenuItemSelector selector) : base(selector)
            {
                OnClick = DoNothing;
                OnFocus = DoNothing;
            }

            public override void Click()
            {
                OnClick();
            }

            public override void Focus()
            {
                OnFocus();
            }

            public override Rect CurrentPosition => currentRect;

            public override bool OnGUI()
            {
                bool clicked = Button(content(), selector.manager.menuItemSizeInInch);
                if (Event.current.type == EventType.Repaint)
                {
                    currentRect = GUILayoutUtility.GetLastRect();
                    currentRect.position -= selector.manager.scrollPos;
                }

                if (clicked)
                {
                    GUI.color = Color.white;
                    OnClick();
                    return true;
                }
                return false;
            }
        }
    }
}