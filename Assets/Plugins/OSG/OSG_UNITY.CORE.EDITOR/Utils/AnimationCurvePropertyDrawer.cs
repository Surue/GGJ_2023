// Old Skull Games
// Pierre Planeau
// Friday, February 15, 2019

// original: https://forum.unity.com/threads/copy-and-paste-curves.162557/#post-1277055

using UnityEngine;
using UnityEditor;

namespace OSG
{
    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurvePropertyDrawer : PropertyDrawer
    {
        private const int buttonWidth = 12; // px
        private const int spaceBetweenCurveAndButtons = 4; // px

        // This script does not copy the curve in the user's clipboard.
        // It stores it in a static variable that will be lost uppon recompilation and Editor restart.

        private static Keyframe[] buffer;
        private static WrapMode preWrapMode;
        private static WrapMode postWrapMode;

        public static void CopyCurve(AnimationCurve curve)
        {
            buffer = curve.keys;
            preWrapMode = curve.preWrapMode;
            postWrapMode = curve.postWrapMode;
        }

        public static AnimationCurve PasteCurve()
        {
            AnimationCurve newAnimationCurve = new AnimationCurve(buffer);
            newAnimationCurve.preWrapMode = preWrapMode;
            newAnimationCurve.postWrapMode = postWrapMode;
            return newAnimationCurve;
        }

        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, property);

            EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width - (buttonWidth * 2) - spaceBetweenCurveAndButtons, pos.height), property, label);
            //property.animationCurveValue = EditorGUI.CurveField(new Rect(pos.x, pos.y, pos.width - (buttonWidth * 2) - spaceBetweenCurveAndButtons, pos.height), label, property.animationCurveValue);

            if (!property.serializedObject.isEditingMultipleObjects)
            {
                // Copy
                if (GUI.Button(new Rect(pos.x + pos.width - buttonWidth * 2, pos.y, buttonWidth, pos.height), ""))
                {
                    CopyCurve(property.animationCurveValue);
                }
                GUI.Label(new Rect(pos.x + pos.width - buttonWidth * 2, pos.y, buttonWidth, pos.height), "C");
            }

            // Paste
            if (buffer != null)
            {
                if (GUI.Button(new Rect(pos.x + pos.width - buttonWidth, pos.y, buttonWidth, pos.height), ""))
                {
                    property.animationCurveValue = PasteCurve();
                }
                GUI.Label(new Rect(pos.x + pos.width - buttonWidth, pos.y, buttonWidth, pos.height), "P");
            }

            EditorGUI.EndProperty();
        }

    }
}
