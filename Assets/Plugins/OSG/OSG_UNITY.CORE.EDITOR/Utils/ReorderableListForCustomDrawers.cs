// Old Skull Games
// Bernard Barthelemy
// Thursday, November 29, 2018

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace OSG
{
    public class ReorderableListForCustomDrawers : ReorderableList
    {
        private static readonly Dictionary<string, ReorderableListForCustomDrawers> allLists =
            new Dictionary<string, ReorderableListForCustomDrawers>();

        private static string GetHashForProperty(SerializedProperty property)
        {
            return property.serializedObject.targetObject.GetHashCode().ToString() 
                   + "::" + property.propertyPath;
        }

        private SerializedProperty ownerProperty;

        private void Validate(SerializedProperty owner, string containerName)
        {

            if(HasBeenDisposed(owner))
            {
                ownerProperty = owner;
                serializedProperty = ownerProperty.FindPropertyRelative(containerName);
            }
        }

        private bool HasBeenDisposed(SerializedProperty owner)
        {
            bool relink;
            try
            {
                // unity sometimes kills the serializedObject under your @ss
                // this should cause an exception
                relink = (owner != ownerProperty) 
                      || !serializedProperty.serializedObject.targetObject
                      || !ownerProperty.serializedObject.targetObject;
            }
            catch (Exception)
            {
                relink = true;
            }

            return relink;
        }

        private bool showList;

        private ReorderableListForCustomDrawers(SerializedObject serializedObject, SerializedProperty containerProperty) 
            : base (serializedObject, containerProperty)
        {
        }


        private ReorderableListForCustomDrawers(string listName, SerializedProperty ownerProperty) 
            : this(ownerProperty.serializedObject, ownerProperty.FindPropertyRelative(listName))
        {
            this.ownerProperty = ownerProperty;
            drawElementCallback = DrawElement;
            elementHeightCallback = ElementHeight;
            drawHeaderCallback = DrawHeader;
            showList = serializedProperty?.arraySize > 0;
        }

        #region overridable stuff

        protected virtual void DrawHeader(Rect rect)
        {
            Rect r = rect;
            float rWidth = 100;
            r.width = rWidth;
            showList = EditorGUI.Foldout(r, showList,
                serializedProperty.arraySize.ToString() + " " + ownerProperty.displayName);
        }

        protected virtual void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            SerializedProperty property = serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, property, new GUIContent(
                $"{serializedProperty.displayName}[{index.ToString()}]"));
        }

        protected virtual float ElementHeight(int index)
        {
            SerializedProperty property = serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(property);
        }

        protected virtual void InternalDoList(Rect position)
        {
            if (showList)
            {
                base.DoList(position);
            }
            else
            {
                this.DrawOnlyHeader(position);
            }
        }

        protected virtual void InternalDoLayout()
        {
            base.DoLayoutList();
        }

        #endregion

        #region public stuff

        public new void DoList(Rect position)
        {
            try
            {
                InternalDoList(position);
            }
            catch (NullReferenceException)
            {
                allLists.Clear();
            }
        }

        public new void DoLayoutList()
        {
            try
            {
                if (showList)
                {
                    InternalDoLayout();
                }
                else
                {
                    this.DrawOnlyHeader();
                }
            }
            catch (NullReferenceException)
            {
                allLists.Clear();
            }
        }

        public virtual float GetPropertyHeight()
        {
            if (!showList)
                return EditorGUIUtility.singleLineHeight;
            float propertyHeight = EditorGUI.GetPropertyHeight(serializedProperty, true);
            return propertyHeight;
        }

        /// <summary>
        /// Creates or retrieve a reorderable list linked to a container (list or array)
        /// belonging to a SerializedProperty.
        /// Usefull for PropertyDrawers
        /// </summary>
        /// <param name="owner">The property of the object to whom the container belongs</param>
        /// <param name="containerName">The name of the container</param>
        /// <returns></returns>

        public static ReorderableListForCustomDrawers GetListFor(SerializedProperty owner,
            string containerName)
        {
            ReorderableListForCustomDrawers reorderableListForCustomDrawers;
            string key = GetHashForProperty(owner);
            if (allLists.TryGetValue(key, out reorderableListForCustomDrawers))
            {
                reorderableListForCustomDrawers.Validate(owner, containerName);
            }

            if (reorderableListForCustomDrawers == null)
            {
                reorderableListForCustomDrawers = new ReorderableListForCustomDrawers(containerName, owner);
                allLists.Add(key, reorderableListForCustomDrawers);
            }

            return reorderableListForCustomDrawers;
        }

        #endregion
    }
}