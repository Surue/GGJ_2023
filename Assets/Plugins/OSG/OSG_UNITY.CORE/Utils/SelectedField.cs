// Old Skull Games
// Bernard Barthelemy
// Friday, August 4, 2017

using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Put On the field which value is used to select the others
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldSelectorAttribute : SelectedField
    {
        public string SelectionFunction;
    }

    /// <summary>
    /// Allows to select fields of a class, in function of the value of one of the fields
    /// (the one that has a FieldSelectorAttribute on it)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SelectedField : PropertyAttribute 
    {
        private readonly int showFor;
        public SelectedField()
        {
            showFor = -1;
        }


        public SelectedField(int types)
        {
            showFor = types;
        }
#if UNITY_EDITOR

        private static SelectedField GetAttribute(FieldInfo field)
        {
            var atts = field.GetCustomAttributes(typeof(SelectedField), false);
            return atts.Length < 1 ? null : atts[0] as SelectedField;
        }

        private delegate SerializedProperty PropertyFinder(string propertyPath);

        private static int GetSelector(Type ownerType, PropertyFinder finder, List<SelectionData> list)
        {
            FieldInfo[] fields = ownerType.GetFields(BindingFlags.Instance
                                                   | BindingFlags.Public 
                                                   | BindingFlags.NonPublic 
                                                   | BindingFlags.FlattenHierarchy);
            int value = -1;
            foreach (FieldInfo field in fields)
            {
                SelectedField att = GetAttribute(field);
                if (att == null) continue;
                SelectionData selectionData = new SelectionData()
                {
                    showFor = att.showFor,
                    property = finder(field.Name)
                };
                list.Add(selectionData);
                FieldSelectorAttribute fsa = att as FieldSelectorAttribute;
                if (fsa != null)
                {
                    if(string.IsNullOrEmpty(fsa.SelectionFunction))
                        value = selectionData.property.intValue;
                    else
                    {
                        object owner = selectionData.property.GetOwner();
                        value = owner.ReflectionInvoke<int>(fsa.SelectionFunction);
                    }
                }
            }
            return value;
        }

        private static List<SelectionData> list  = new List<SelectionData>();

        private struct SelectionData
        {
            public int showFor;
            public SerializedProperty property;
        }

        public static void Enumerate(SerializedObject serializedObject, Action<SerializedProperty> doAction)
        {
            int selector = GetSelector(serializedObject.targetObject.GetType(),
                serializedObject.FindProperty, list);
            Enumerate(doAction, selector);
        }

        public static void Enumerate(SerializedProperty property, 
            Type ownerType,
            Action<SerializedProperty> doAction)
        {
            int selector = GetSelector(ownerType, property.FindPropertyRelative, list );
            Enumerate(doAction, selector);
        }

        private static void Enumerate(Action<SerializedProperty> doAction, int selector)
        {
            foreach (SelectionData data in list)
            {
                if (IsExcluded(data.showFor, selector)) continue;
                doAction(data.property);
            }
            list.Clear();
        }

        private static bool IsExcluded(int value, int selector)
        {
            return (value & selector) == 0 && selector != 0;
        }
#endif
    }
}