using UnityEditor;
using UnityEngine;

namespace OSG
{
    public static class HandlesPreferences
    {
        [EditorPrefs] private static bool handleConstantInWorld;
        [EditorPrefs] private static float handleSize = 0.3f;
        public static float Size(Vector3 worldPos)
        {
            return handleConstantInWorld 
                ? handleSize
                : HandleUtility.GetHandleSize( worldPos ) * handleSize;
        }
    }
}