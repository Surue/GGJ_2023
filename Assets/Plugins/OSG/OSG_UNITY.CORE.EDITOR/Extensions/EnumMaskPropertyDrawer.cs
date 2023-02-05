// Old Skull Games
// Benoit Constantin
// Tuesday, October 01, 2019

using System;
using OSG.Core;
using UnityEngine;
using UnityEditor;

namespace OSG
{
    [CustomPropertyDrawer(typeof(EnumExtensions.EnumMask), true)]
    public class EnumMaskPropertyDrawer : PropertyDrawer
    {

        private bool foldoutOpen;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (foldoutOpen)
                return EditorGUIUtility.singleLineHeight * (Enum.GetValues(((EnumExtensions.IUnderlyingTypeProvider)fieldInfo.GetValue(property.serializedObject.targetObject)).GetUnderlyingType()).Length + 2) + 10;
            else
                return EditorGUIUtility.singleLineHeight;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object theEnum = fieldInfo.GetValue(property.serializedObject.targetObject);

            Type enumUnderlyingType = ((EnumExtensions.IUnderlyingTypeProvider)theEnum).GetUnderlyingType();


                EditorGUI.BeginProperty(position, label, property);

                foldoutOpen = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), foldoutOpen, label);

                if (foldoutOpen)
                {
                    //Draw the All button
                    if (GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 1 + 5, 30, 15), "All"))
                    {
                        property.FindPropertyRelative("mask").intValue = EnumExtensions.SetAllBit(enumUnderlyingType);
                    }

                    //Draw the None button
                    if (GUI.Button(new Rect(position.x + 32, position.y + EditorGUIUtility.singleLineHeight * 1 + 5, 50, 15), "None"))
                    {
                        property.FindPropertyRelative("mask").intValue = 0;
                    }

                    string[] enumNames = Enum.GetNames(enumUnderlyingType);

                    //Draw the list
                    for (int i = 0; i < enumNames.Length; i++)
                    {
                        SerializedProperty mask = property.FindPropertyRelative("mask");
                        if (EditorGUI.Toggle(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * (2 + i) + 5, position.width, EditorGUIUtility.singleLineHeight), enumNames[i], EnumExtensions.IsSet(mask.intValue, i)))
                        {
                            mask.intValue = EnumExtensions.SetBitMask(mask.intValue, i);
                        }
                        else
                        {
                            mask.intValue = EnumExtensions.UnSetBitMask(mask.intValue, i);
                        }
                    }

                }

                fieldInfo.SetValue(property.serializedObject.targetObject, theEnum);
                property.serializedObject.ApplyModifiedProperties();

                EditorGUI.EndProperty();
        }

    }
}