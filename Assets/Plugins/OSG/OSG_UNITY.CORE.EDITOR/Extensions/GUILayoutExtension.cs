// Old Skull Games
// Benoit Constantin
// Tuesday, February 05, 2019

using UnityEngine;
using UnityEditor;
using System;

namespace OSG
{
    public static class GUILayoutExtension
    {
        /// <summary>
        /// Center a LayoutCall
        /// </summary>
        /// <param name="layoutCall"></param>
        public static void CenterLayout(Action layoutCall, params GUILayoutOption[] guiLayoutOptions)
        {
            CenterLayout(layoutCall, GUIStyle.none, guiLayoutOptions);
        }


        /// <summary>
        /// Center a LayoutCall
        /// </summary>
        /// <param name="layoutCall"></param>
        public static void CenterLayout(Action layoutCall,GUIStyle guiStyle, params GUILayoutOption[] guiLayoutOptions)
        {
            GUILayout.BeginHorizontal(guiStyle, guiLayoutOptions);
            {
                GUILayout.FlexibleSpace();
                layoutCall();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }


        private static ICloneable copiedObject;
        public static void CopyPasteButton<T>(ref T c, GUIStyle guiStyle, params GUILayoutOption[] guiLayoutOptions) where T : ICloneable
        {
            GUILayout.BeginHorizontal(guiStyle, guiLayoutOptions);
            {
                if(GUILayout.Button("C"))
                    copiedObject = c;

                if(copiedObject != null && copiedObject.GetType() == typeof(T))
                {
                    if (GUILayout.Button("P"))
                        c = (T)copiedObject.Clone();
                }
            }
            GUILayout.EndHorizontal();
        }


        public static void CopyPasteButton<T>(ref T c,params GUILayoutOption[] guiLayoutOptions) where T : ICloneable
        {
            CopyPasteButton(ref c, GUIStyle.none, guiLayoutOptions);
        }
    }
}
