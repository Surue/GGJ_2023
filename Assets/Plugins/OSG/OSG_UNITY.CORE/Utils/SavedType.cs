using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Allow to declare a variable of a given type, that will be saved, for example in Player's data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SavedType<T> 
    {
        protected bool Equals(SavedType<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SavedType<T>) obj);
        }

        public override int GetHashCode()
        {
            return Key != null ? Key.GetHashCode() : 0;
        }

        protected virtual void CommitSave(){}

        public virtual bool HasValue { protected set; get; }
        private T value;
        protected readonly T defaultValue;

        protected delegate void SaveDelegate(string key, T value);

        protected delegate T LoadDelegate(string key, T defaultValue);

        protected readonly string Key;
        private readonly SaveDelegate Save;
        private readonly LoadDelegate Load;

        public override string ToString()
        {
            return Key + " " + Value.ToString().InColor(Color.green);
        }

        [Obsolete("Don't compare 2 SavedType directly, compare their values", true)]
        public static bool operator ==(SavedType<T> a, SavedType<T> b)
        {
            throw new Exception("Don't use that");
        }

        [Obsolete("Don't compare 2 SavedType directly, compare their values", true)]
        public static bool operator !=(SavedType<T> a, SavedType<T> b)
        {
            throw new Exception("Don't use that");
        }


        public static implicit operator T (SavedType<T> t)
        {
            return t.Value;
        }

        protected SavedType(string key, T defaultValue, SaveDelegate save, LoadDelegate load)
        {
            Key = key;
            this.defaultValue = defaultValue;
            Save = save;
            Load = load;
        }

        public T Value
        {
            get
            {
                if(!HasValue)
                {
                    value = Load(Key, defaultValue);
                    HasValue = true;
                }
                return value;
            }
            set
            {
                this.value = value;
                HasValue = true;
                Save(Key, value);
                CommitSave();
            }
        }
    }


    public abstract class SavedPlayerData<T> : SavedType<T>
    {
        protected SavedPlayerData(string key, T defaultValue, SaveDelegate save, LoadDelegate load) : base(key, defaultValue, save, load)
        {
        }

        protected override void CommitSave()
        {
            PlayerPrefs.Save();
        }
    }


    public class SavedBool : SavedPlayerData<bool>
    {
        public SavedBool(string key, bool defaultValue=false) 
            : base(key,defaultValue, 
            (s, b) => PlayerPrefs.SetInt(s, b ? 1 : 0), (s, b) => PlayerPrefs.GetInt(s, defaultValue ? 1 : 0) == 1){
        }

    }

    public class SavedInt : SavedPlayerData<int>
    {
        public SavedInt(string key, int defaultValue = 0) 
            : base(key, defaultValue, 
            PlayerPrefs.SetInt, PlayerPrefs.GetInt){}
    }

    public class SavedFloat : SavedPlayerData<float>
    {
        public SavedFloat(string key, float defaultValue = 0) 
            : base(key, defaultValue, PlayerPrefs.SetFloat, PlayerPrefs.GetFloat){}
    }

    public class SavedString : SavedPlayerData<string>
    {
        public SavedString(string key, string defaultValue ="") 
            : base(key, defaultValue, PlayerPrefs.SetString, PlayerPrefs.GetString){}
    }

}