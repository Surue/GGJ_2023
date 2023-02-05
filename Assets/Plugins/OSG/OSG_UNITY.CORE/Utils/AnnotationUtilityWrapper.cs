// Old Skull Games
// Bernard Barthelemy
// Tuesday, September 19, 2017

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
#endif

namespace OSG
{
    /// <summary>
    /// Dirty trick to get 3D gizmos size in scene editor
    /// </summary>
    public static class AnnotationUtilityWrapper
    {
#if UNITY_EDITOR
        static System.Type m_AnnotationUtilityType;
        static System.Reflection.PropertyInfo m_IconSize;
        static AnnotationUtilityWrapper()
        {
            m_AnnotationUtilityType = typeof(Editor).Assembly.GetTypes().Where(t=>t.Name == "AnnotationUtility").FirstOrDefault();
            if (m_AnnotationUtilityType == null)
            {
                UnityEngine.Debug.LogWarning("The internal type 'AnnotationUtility' could not be found. Maybe something changed inside Unity");
                return;
            }
            m_IconSize = m_AnnotationUtilityType.GetProperty("iconSize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (m_IconSize == null)
            {
                UnityEngine.Debug.LogWarning("The internal class 'AnnotationUtility' doesn't have a property called 'iconSize'");
            }
        }
        public static float IconSize
        {
            get { return (m_IconSize == null) ? 0.5f : (float)m_IconSize.GetValue(null, null); }
            set { if (m_IconSize != null) m_IconSize.SetValue(null, value, null); }
        }
        public static float IconSizeLinear
        {
            get { return ConvertTexelWorldSizeTo01(IconSize); }
            set { IconSize = Convert01ToTexelWorldSize(value); }
        }
        public static float Convert01ToTexelWorldSize(float value01)
        {
            if (value01 <= 0f)
            {
                return 0f;
            }
            return Mathf.Pow(10f, -3f + 3f * value01);
        }
        public static float ConvertTexelWorldSizeTo01(float texelWorldSize)
        {
            if (texelWorldSize == -1f)
            {
                return 1f;
            }
            if (texelWorldSize == 0f)
            {
                return 0f;
            }
            return (Mathf.Log10(texelWorldSize) - -3f) / 3f;
        }
#endif
    }
}