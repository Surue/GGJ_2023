// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020


using UnityEngine;

namespace OSG
{
    public partial class BaseCheatManager
    {
        public class CustomMenuItem : BaseMenuItem
        {
            public delegate bool GUIDelegate();

            private GUIDelegate guiFunc;
            private Rect _currentPosition;

            public override void Click()
            {
            }

            public override void Focus()
            {
            }

            public CustomMenuItem(BaseMenuItemSelector selector, GUIDelegate gui) : base(selector)
            {
                guiFunc = gui;
            }

            public override Rect CurrentPosition => _currentPosition;

            public override bool OnGUI()
            {
                return guiFunc();
            }
        }
    }
}