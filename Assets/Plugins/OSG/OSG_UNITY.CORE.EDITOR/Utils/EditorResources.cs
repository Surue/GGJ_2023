using UnityEngine;
using UnityEditor;

namespace OSG
{
    public class EditorResources : EditorSingletonScriptableObject<EditorResources>
    {
        public Texture2D placeholderIcon;
        public Texture2D eyeIcon;
        public Texture2D barredEyeIcon;
        public Texture2D plusIcon;
        public Texture2D minusIcon;
        public Texture2D crossIcon;
        public Texture2D arrowUpIcon;
        public Texture2D arrowDownIcon;
        public Texture2D arrowLeftIcon;
        public Texture2D arrowRightIcon;
    }
}