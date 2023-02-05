// Old Skull Games
// Benoit Constantin
// Monday, April 01, 2019


using UnityEngine;
using UnityEditorInternal;
using System.Collections;
using System;
using UnityEditor;

namespace OSG
{
    /// <summary>
    /// utility class to fix ReorderableDelegate
    /// </summary>
    public class OSGReorderableList<T> : ReorderableList {

        /// <summary>
        /// To add some context information (like informations on element...)
        /// </summary>
        public T contextInfo;

        public OSGReorderableList(IList elements, Type elementType) : base(elements, elementType) { base.drawElementCallback += DrawElementCallback; }
        public OSGReorderableList(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements) { base.drawElementCallback += DrawElementCallback; }
        public OSGReorderableList(IList elements, Type elementType, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) : base(elements, elementType, draggable, displayHeader, displayAddButton, displayRemoveButton) { base.drawElementCallback += DrawElementCallback; }
        public OSGReorderableList(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton) { base.drawElementCallback += DrawElementCallback; }

        /// <summary>
        /// Fixed drawElementCallback
        /// rect / index / isActive / isFocused
        /// </summary>
        public Action<OSGReorderableList<T>, Rect, int, bool, bool> OnDrawElement;

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (OnDrawElement != null)
                OnDrawElement(this, rect, index, isActive, isFocused);
            else
                ReorderableList.defaultBehaviours.DrawElement(rect, this.serializedProperty.GetArrayElementAtIndex(index), (object)null, false, false, this.draggable);
        }

    }
}