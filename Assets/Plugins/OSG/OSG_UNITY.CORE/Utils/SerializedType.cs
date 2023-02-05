// // Old Skull Games
// // Bernard Barthelemy
// // 14:57

using System;
using UnityEngine;

namespace OSG
{
    [Serializable]
    public abstract class SerializedType : ISerializationCallbackReceiver
    {
        [SerializeField] protected string fullTypeName;
        public abstract Type baseType { get; }
        protected Type effectiveType;
        public void SetEffectiveType(Type t) => effectiveType = t;

        public static implicit operator Type(SerializedType st)
        {
            return st.effectiveType ?? st.baseType;
        }

        #region Implementation of ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            fullTypeName = effectiveType?.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            if(!string.IsNullOrEmpty(fullTypeName))
            {
                effectiveType = Type.GetType(fullTypeName);
            }
        }

        #endregion
    }


    /// <summary>
    /// Allow to serialize in an Unity Object, a variable of class Type
    /// 
    /// For example, if you want to serialize which type of Character to spawn,
    /// 
    /// 1. you define a CharacterSerializedType, to allow the custom property drawer to work
    ///    [Serializable] class CharacterSerializedType : SerializeType<Character> {}
    /// 
    /// 2. then use it in your object's class
    ///    [SerializeField] CharacterSerializedType characterType;
    ///
    /// </summary>
    /// <typeparam name="T">The class the variable Type must be or be deriving from</typeparam>
    
    [Serializable]
    public abstract class SerializedType<T>: SerializedType
    {
        public override Type baseType => typeof(T);
    }
}