using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Helper class.
    /// Use to display test UI
    /// Theses methods have to be called in a OnGUI method.
    /// </summary>
    public static class TestUITools
    {
        static int x = 0;
        static int y = 0;

        /// <summary>
        /// Initialises class.
        /// </summary>
        public static void BeginUI()
        {
            x = 0;
            y = 0;
        }

        /// <summary>
        /// Displays a button.
        /// </summary>
        /// <returns><c>true</c>, if button was clicked, <c>false</c> otherwise.</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="title">Title.</param>
        public static bool DisplayButton(int width, int height, string title)
        {
            bool b = GUI.Button(new Rect(x + 10, y + 10, width, height), title);
            x += width + 20;
            return b;
        }

        /// <summary>
        /// Displays a toggle.
        /// </summary>
        /// <returns><c>true</c>, if toggle was checked, <c>false</c> otherwise.</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="value">If set to <c>true</c> toggle is checked.</param>
        /// <param name="title">Toggle text.</param>
        public static bool DisplayToggle(int width, int height, bool value, string title)
        {
            bool b = GUI.Toggle(new Rect(x + 10, y + 10, width, height), value, title);
            x += width + 20;
            return b;
        }

        /// <summary>
        /// Displays a label.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="value">Value.</param>
        public static void DisplayLabel(int width, int height, string value)
        {
            GUI.Label(new Rect(x + 10, y + 10, width, height), value);
            x += width + 20;
        }

        /// <summary>
        /// Displaies an horizontal slider.
        /// </summary>
        /// <returns>New Value</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="value">Value.</param>
        /// <param name="leftValue">Left value.</param>
        /// <param name="rightValue">Right value.</param>
        public static float DisplayHorizontalSlider(int width, int height, float value, float leftValue, float rightValue)
        {
            float newValue =  GUI.HorizontalSlider(new Rect(x + 10, y + 15, width, height), value, leftValue, rightValue);
            x += width + 20;
            return newValue;
        }

        /// <summary>
        /// Begins a new UI line.
        /// </summary>
        /// <param name="lineHeight">Line height.</param>
        public static void NewLine(int lineHeight)
        {
            x = 0;
            y += lineHeight;
        }
    }
}

