// Old Skull Games
// Bernard Barthelemy
// Thursday, March 5, 2020

using UnityEngine;

namespace OSG
{
    public partial class CallBuilder
    {
        public abstract class CallBuilderSimpleTypeSelector : CallBuilderMenuItemSelector
        {
            protected CallBuilderSimpleTypeSelector(CallBuilder builder, string name) : base(builder, name)
            {
            }
        }

        public class CallBuilderSimpleTypeSelector<T> : CallBuilderSimpleTypeSelector
        {
            private readonly GUIFunc<T> gui;

            public CallBuilderSimpleTypeSelector(CallBuilder builder, GUIFunc<T> gui, string name) : base(builder, name)
            {
                this.gui = gui;
            }

            public override void OnGUI()
            {
                if (state != State.Building)
                    return;
                Event current = Event.current;
                if (current == null)
                {
                    return;
                }

                bool validate = current.type == EventType.KeyUp &&
                                (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);

                if (BaseCheatManager.Button(new GUIContent("Enter " + name), Size))
                    validate = true;

                GUI.SetNextControlName("Totor");
                validate = gui(Size, validate, out var param);
                GUI.FocusControl("Totor");

                if (validate)
                {
                    builder.AddParameter(param);
                    state = State.Ready;
                }
                else if (current.keyCode == KeyCode.Escape && current.isKey)
                {
                    state = State.Cancel;
                }
            }
        }

    }
}