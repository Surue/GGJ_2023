using UnityEngine;

namespace OSG
{
    public class ToggleButtons : PropertyAttribute
    {
        public readonly string option1;
        public readonly string option2;

        public readonly string label;

        public ToggleButtons(string option1, string option2, string label = default)
        {
            this.option1 = option1;
            this.option2 = option2;

            this.label = label;
        }
    }

    public class Box : PropertyAttribute
    {
        public readonly float spacing = 2f;
        public readonly int thickness = 1;

        public Box(int thickness)
        {
            this.thickness = thickness;
        }

        public Box (float spacing = 2f, int thickness = 1)
        {
            if (thickness < 1)
                thickness = 1;

            this.spacing = spacing;
            this.thickness = thickness;
        }
    }

    public class OnOff : PropertyAttribute
    {
        public readonly string boolName;
        public readonly bool enableWithBool;
        public readonly string label;

        public OnOff(string boolName, string label)
        {
            this.boolName = boolName;
            this.enableWithBool = false;
            this.label = label;
        }

        public OnOff(string boolName, bool enableWithBool = false, string label = default)
        {
            this.boolName = boolName;
            this.enableWithBool = enableWithBool;
            this.label = label;
        }
    }

    public class Label : PropertyAttribute
    {
        public readonly string label;

        public Label(string label)
        {
            this.label = label;
        }
    }
}