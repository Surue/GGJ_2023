
using System;
using UnityEditor;
using UnityEngine;

namespace OSG.Properties
{
    public abstract class PropertyFrontend<T>
    {
        protected SerializedProperty property;

        protected Func<T> get;
        protected Action<T> set;

        protected PropertyFrontend(string propertyName, Editor editor)
        {
            property = editor.serializedObject.FindProperty(propertyName);
        }

        public static implicit operator T(PropertyFrontend<T> p)
        {
            return p.Get();
        }

        public T Get()
        {
            return get();
        }

        public void Set(T value)
        {
            set(value);
        }

    }


    public class IntPropertyFrontend : PropertyFrontend<int>
    {
        public IntPropertyFrontend(string propertyName, Editor editor) : base(propertyName, editor)
        {
            get = () => property.intValue;
            set = i => property.intValue = i;
        }
    }


    public class ObjectPropertyFrontend<T> : PropertyFrontend<T> where T : UnityEngine.Object
    {
        public ObjectPropertyFrontend(string propertyName, Editor editor) : base(propertyName, editor)
        {
            get = () => property.objectReferenceValue as T;
            set = o => property.objectReferenceValue = o;
        }
    }

    public class ColorPropertyFrontend : PropertyFrontend<Color>
    {
        public ColorPropertyFrontend(string propertyName, Editor editor) : base(propertyName, editor)
        {
            get = () => property.colorValue;
            set = color => property.colorValue = color;
        }
    }



}