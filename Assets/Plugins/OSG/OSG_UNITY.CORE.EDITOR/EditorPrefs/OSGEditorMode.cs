// Old Skull Games
// Pierre Planeau
// Friday, October 27, 2017

namespace OSG
{
    public static class OSGEditorMode
    {
        public enum Mode
        {
            /// <summary>
            /// Only shows specific Components.
            /// </summary>
            Simplified = 0,
            /// <summary>
            /// Only shows usefull components to Level Designers.
            /// </summary>
            LevelDesign = 16,
            /// <summary>
            /// Shows everything.
            /// </summary>
            Complete = 128
        }

        /// <summary>
        /// The currently selected OSGEditorMode.
        /// </summary>
        [EditorPrefs]
        public static Mode selected = Mode.Complete;
    }
}
