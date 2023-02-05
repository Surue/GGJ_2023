// Old Skull Games
// Bernard Barthelemy
// Thursday, June 1, 2017

using UnityEngine;

namespace OSG
{
    public class EditorPrefsAttribute : PropertyAttribute
    {
        public readonly string label;
        public bool showGUI=true;
        public string onChangeCallback;
        public EditorPrefsAttribute(string label)
        {
            this.label = label;
        }

        public EditorPrefsAttribute() : this(null)
        {
        }
    }
}