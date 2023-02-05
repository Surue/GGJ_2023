#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OSG
{
    public static class SerializedPropertyExtensions
    {
        private static GUIStyle _dbs;
        public static GUIStyle DropButtonStyle => _dbs ?? (_dbs = new GUIStyle(EditorStyles.popup)
                                                       {alignment = TextAnchor.MiddleCenter});
        private class PropertyVisitor
        {
            private class Container
            {
                private readonly object o;
                private readonly IList list;
                public Container(object container)
                {
                    o = container;
                    list = o as IList;
                }

                public object GetValue(int index)
                {
                    if (list != null)
                        return list[index];

                    throw new Exception("Container Type not managed : " + o.GetType());
                }

                public void SetValue(object value, int index)
                {
                    if (list != null)
                    {
                        list[index] = value;
                        return;
                    }

                    throw new Exception("Container Type not managed : " + o.GetType());
                }
            }

            List<string> path;
            object currentOwner;
            Container currentContainer;
            string currentToken;
            FieldInfo currentFieldInfo;

            public PropertyVisitor(SerializedProperty property)
            {
                path = Enumerable.ToList(property.propertyPath.Split('.'));
                currentOwner = property.serializedObject.targetObject;
            }

            #region UTILS

            private bool IsData
            {
                get { return currentToken.StartsWith("data"); }
            }

            private bool IsArray
            {
                get { return currentToken == "Array"; }
            }

            private int GetArrayIndex()
            {
                GetToken();
                if (!IsData)
                    return -1;

                int i0 = currentToken.IndexOf('[') + 1;
                if (i0 <= 0)
                    return -1;
                int i1 = currentToken.IndexOf(']');
                if (i1 < 0)
                    return -1;

                int length = i1 - i0;
                string indexString = currentToken.Substring(i0, length);
                int index;
                if (!int.TryParse(indexString, out index))
                {
                    return -1;
                }
                currentContainer = new Container(currentOwner);
                return index;
            }

            private void GetToken()
            {
                if (path.Any())
                {
                    currentToken = path[0];
                    path.RemoveAt(0);
                }
                else
                {
                    currentToken = "";
                }
            }

            private bool IsFinished
            {
                get { return path.Count <= 0; }
            }

            public object Owner { get { return currentOwner; } }

            private void GetOwnerFieldInfo()
            {
                currentFieldInfo = currentOwner.GetType().GetField(currentToken, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            private void GetNextOwner()
            {
                GetOwnerFieldInfo();
                if (currentFieldInfo != null)
                    currentOwner = currentFieldInfo.GetValue(currentOwner);
            }

            #endregion
            #region GET VALUE

            public bool GetObjectValue<T>(ref T value)
            {
                GetToken();
                if (IsArray)
                {
                    return GetObjectValueInArray(ref value);
                }
                GetNextOwner();
                if (currentFieldInfo == null)
                    return false;

                if (IsFinished)
                {
                    if (currentOwner is T)
                    {
                        value = (T)currentOwner;
                        return true;
                    }
                    return false;
                }

                return GetObjectValue(ref value);
            }


            private bool GetObjectValueInArray<T>(ref T value)
            {
                int index = GetArrayIndex();
                if (index < 0)
                    return false;

                currentOwner = currentContainer.GetValue(index);
                if (IsFinished)
                {
                    value = (T)currentOwner;
                    return true;
                }
                return GetObjectValue(ref value);
            }

            #endregion
            #region SET VALUE


            public bool SetObjectValue(object value, bool setNew)
            {
                GetToken();
                if (IsArray)
                {
                    return SetObjectValueInArray(value, setNew);
                }

                GetOwnerFieldInfo();
                if (currentFieldInfo == null)
                {
                    return false;
                }

                if (IsFinished)
                {
                    Type elementType = currentOwner.GetType();

                    currentFieldInfo.SetValue(currentOwner, setNew ? elementType.GetNewValue() : value);
                    return true;
                }

                currentOwner = currentFieldInfo.GetValue(currentOwner);
                return SetObjectValue(value, setNew);
            }



            private bool SetObjectValueInArray(object value, bool setNew)
            {
                int index = GetArrayIndex();
                if (index < 0)
                    return false;

                Type ownerType = currentOwner.GetType();
                //                if (!ownerType.IsArray)
                //                    throw new Exception("Expecting an array... got " + ownerType.Name);
                Type elementType = ownerType.GetElementType();
                if (IsFinished)
                {
                    currentContainer.SetValue(setNew ? elementType.GetNewValue() : value, index);
                    return true;
                }

                if (elementType != null && elementType.IsValueType)
                {
                    // from here if we have an array of structs, we should back propagate any change made if we'd called SetObjectValueInternal recursively.
                    // might not be needed ...
                    UnityEngine.Debug.LogError("Can't manage properties within arrays of structs");
                    return false;
                }
                currentOwner = currentContainer.GetValue(index);
                return SetObjectValue(value, setNew);
            }
            #endregion

            #region GET OWNER

            public object GetOwner()
            {
                GetToken();
                if (IsFinished)
                {
                    return currentOwner;
                }
                if (IsArray)
                {
                    return GetOwnerInArray();
                }

                GetNextOwner();
                return GetOwner();
            }

            private object GetOwnerInArray()
            {
                int index = GetArrayIndex();
                if (index < 0)
                    return null;
                currentOwner = currentContainer.GetValue(index);
                return IsFinished ? currentOwner : GetOwner();
            }

            #endregion
            #region GET FIELD INFO
            public FieldInfo GetFieldInfo()
            {
                GetToken();
                if (IsArray)
                {
                    return GetFieldInfoInArray();
                }
                GetNextOwner();
                if (IsFinished)
                {
                    return currentFieldInfo;
                }
                return GetFieldInfo();
            }

            private FieldInfo GetFieldInfoInArray()
            {
                int index = GetArrayIndex();
                if (index < 0)
                    return null;
                currentOwner = currentContainer.GetValue(index);
                return GetFieldInfo();

            }

            #endregion
        }


        public static bool GetObjectValue<T>(this SerializedProperty property, out T value)
        {
            var visitor = new PropertyVisitor(property);
            value = default(T);
            return visitor.GetObjectValue(ref value);
        }

        public static bool SetObjectValue<T>(this SerializedProperty property, T value)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            var visitor = new PropertyVisitor(property);
            return visitor.SetObjectValue(value, false);
        }

        public static bool SetNewValue(this SerializedProperty property)
        {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            var visitor = new PropertyVisitor(property);
            return visitor.SetObjectValue(null, true);
        }


        public static object GetOwner(this SerializedProperty property)
        {
            var visitor = new PropertyVisitor(property);
            return visitor.GetOwner();
        }

        public static object GetNewValue(this Type elementType)
        {
            if (elementType == null)
                return null;
            if (elementType == typeof(string))
            {
                return "";
            }

            if (elementType.IsValueType)
            {
                return Activator.CreateInstance(elementType);
            }

            if (elementType.IsAbstract)
                return null;


            // don't new a UnityEngine.Object... 
            if (elementType.DerivesFrom(typeof(UnityEngine.Object)))
                return null;


            if (elementType.GetConstructor(Type.EmptyTypes) == null)
            {
                UnityEngine.Debug.LogError((object)("Cannot add element. Type " + elementType.ToString() +
                                                     " has no default constructor. Implement a default constructor or implement your own add behaviour."
                ));
                return null;
            }

            Type[] genericArguments = elementType.GetGenericArguments();
            return genericArguments.Length > 0 && genericArguments[0] != null
                ? Activator.CreateInstance(genericArguments[0])
                : Activator.CreateInstance(elementType);
        }


        //     public static FieldInfo GetFieldInfoAndOwner<T>(this SerializedProperty property, out T result)
        //     {
        //         PropertyVisitor visitor = new PropertyVisitor(property);
        //         result = default(T);
        //         FieldInfo info = visitor.GetFieldInfo();
        //         result = (T)visitor.Owner ;
        //         return info;
        //     }


        /// <summary>
        /// Get the FieldInfo and target object corresponding to the given SerializedProperty
        /// to be used in case said property doesn't allow direct access to 
        /// its target object. I.E. when the target object isn't a UnityEngine.Object
        /// or a base type.
        /// This is WIP and mau not be 
        /// </summary>
        /// <param name="property"> The property </param>
        /// <param name="obj">the target obj (possibly null)</param>
        /// <returns>The FieldInfo</returns>

        [Obsolete("This function is crap, don't use it!", false)]
        // TODO : use PropertyVisitor
        public static FieldInfo GetFieldInfoAndOwner<T>(this SerializedProperty property, out T result) where T : class
        {
            string[] path = property.propertyPath.Split('.');
            object obj = property.serializedObject.targetObject;
            Type containerType = obj.GetType();
            FieldInfo field = null;
            int lastIndex = path.Length - 1;
            result = null;

            bool isArray = false;
            int index = -1;

            for (int i = 0; i <= lastIndex; ++i)
            {
                if (path[i] == "Array")
                {
                    isArray = true;
                    continue;
                }

                if (isArray)
                {
                    if (path[i].StartsWith("data"))
                    {
                        int i0 = path[i].IndexOf('[') + 1;

                        int length = path[i].IndexOf(']') - i0;
                        string indexString = path[i].Substring(i0, length);
                        if (!int.TryParse(indexString, out index))
                        {
                            return null;
                        }
                    }

                    if (index >= 0)
                    {
                        Array array = obj as Array;
                        if (array != null)
                        {
                            if (index >= array.Length)
                                return null;
                            obj = array.GetValue(index);
                            containerType = obj.GetType();
                        }
                        else
                        {
                            // TODO : List<SomethingThatHas<T>>
                            if (i != lastIndex)
                                return null;
                            List<T> l = obj as List<T>;
                            if (l == null)
                                return null;
                            if (index >= l.Count)
                                return null;
                            obj = l[index];
                        }
                        //field = GetField(containerType, path[i]);
                        index = -1;
                        isArray = false;
                    }
                }
                else
                {
                    field = GetField(containerType, path[i]);
                    if (field != null)
                    {
                        if (obj != null && i < lastIndex)
                        {
                            obj = field.GetValue(obj);
                        }
                        containerType = field.FieldType;
                    }
                }

                if (field == null)
                    return null;
            }
            result = obj as T;
            return field;
        }

        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            PropertyVisitor visitor = new PropertyVisitor(property);
            return visitor.GetFieldInfo();
        }


        private static FieldInfo GetField(Type containerType, string path)
        {
            return containerType.GetField(path, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }


        public static bool GenericBoolEditorField(this SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.boolValue = (EditorGUILayout.Toggle(propertyName, property.boolValue));
            return property.boolValue;
        }

        public static bool GenericBoolEditorField(this SerializedProperty serializedProperty, string propertyName)
        {
            SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);
            property.boolValue = (EditorGUILayout.Toggle(propertyName, property.boolValue));

            return property.boolValue;
        }

        public static UnityEngine.Object GenericObjectEditorField(this SerializedObject serializedObject, string propertyName, Type type, bool allowSceneObject)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.objectReferenceValue =(EditorGUILayout.ObjectField(propertyName, property.objectReferenceValue, type, allowSceneObject));

            return property.objectReferenceValue;
        }

        public static void GenericPropertyEditorField(this SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
             EditorGUILayout.PropertyField(property);
        }

        public static string GenericStringEditorField(this SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.stringValue = (EditorGUILayout.TextField(propertyName, property.stringValue));

            return property.stringValue;
        }


        public static void GenericStringPopupEditorField(this SerializedObject serializedObject, string propertyName, string[] popup)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            string val = property.stringValue;

            int index = 0;
            for (int i = 0; i < popup.Length; i++)
            {
                if (val == popup[i])
                {
                    index = i;
                    break;
                }
            }

            int newIndex = EditorGUILayout.Popup(property.name, index, popup);
            property.stringValue = popup[newIndex];
        }


        public static void GenericPropertyEditorField(this SerializedProperty serializedProperty, string propertyName)
        {
            SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);
            EditorGUILayout.PropertyField(property);
        }


        public static string GenericStringEditorField(this SerializedProperty serializedProperty, string propertyName)
        {
            SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);
            property.stringValue = (EditorGUILayout.TextField(propertyName, property.stringValue));

            return property.stringValue;
        }

#if UNITY_2019_1_OR_NEWER
        public static int GenericEnumEditorField<T>(this SerializedProperty serializedProperty, string propertyName) where T : Enum
        {
            SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);

            object values = Enum.GetValues(typeof(T)).GetValue(property.enumValueIndex);
            if (values == null)
                values = default(T);

            property.enumValueIndex = (int)(object)EditorGUILayout.EnumPopup(propertyName, (T)values);

            return property.enumValueIndex;
        }
#endif


        public static float GenericFloatEditorField(this SerializedProperty serializedProperty, string propertyName)
        {
            SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);
            property.floatValue = (EditorGUILayout.FloatField(propertyName, property.floatValue));

            return property.floatValue;
        }


        public static void GenericStringPopupEditorField(this SerializedProperty serializedProperty, string propertyName, string[] popup)
        {
            SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);

            string val = property.stringValue;

            int index = 0;
            for (int i = 0; i < popup.Length; i++)
            {
                if(val == popup[i])
                {
                    index = i;
                    break;
                }
            }

            int newIndex = EditorGUILayout.Popup(propertyName, index, popup);

           property.stringValue = popup[newIndex];
        }


        public static void GenericStringPopupEditorField(this SerializedProperty serializedProperty, string[] popup)
        {
            string val = serializedProperty.stringValue;

            int index = 0;
            for (int i = 0; i < popup.Length; i++)
            {
                if (val == popup[i])
                {
                    index = i;
                    break;
                }
            }

            int newIndex = EditorGUILayout.Popup(serializedProperty.displayName, index, popup);
            serializedProperty.stringValue = popup[newIndex];
        }


        /// <summary>
        /// Returns true is the SerializedProperty has not been created or assigned.
        /// </summary>
        public static bool IsNull(this SerializedProperty serializedProperty)
        {
            return serializedProperty == null;
        }


        /// <summary>
        /// Returns true if all this array elements are expanded.
        /// </summary>
        public static bool IsArrayFullyExpanded(this SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                return serializedProperty.isExpanded;

            int count = serializedProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                if (!serializedProperty.GetArrayElementAtIndex(i).isExpanded)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if all this array elements are shrinked.
        /// </summary>
        public static bool IsArrayFullyCollapsed(this SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                return !serializedProperty.isExpanded;

            int count = serializedProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                if (serializedProperty.GetArrayElementAtIndex(i).isExpanded)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Expands all array elements.
        /// </summary>
        public static void ExpandArray(this SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                return;

            int count = serializedProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                serializedProperty.GetArrayElementAtIndex(i).isExpanded = true;
            }
        }

        /// <summary>
        /// Shrinks all array elements.
        /// </summary>
        public static void CollapseArray(this SerializedProperty serializedProperty)
        {
            if (!serializedProperty.isArray)
                return;

            int count = serializedProperty.arraySize; for (int i = 0; i < count; i++)
            {
                serializedProperty.GetArrayElementAtIndex(i).isExpanded = false;
            }
        }
    }
}
#endif