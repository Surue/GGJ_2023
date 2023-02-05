// Old Skull Games
// Pierre Planeau
// Wednesday, September 13, 2017
#define CHECK_SAVE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSG.Core.EventSystem;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Base class for saving and loading data from any source (PlayerPrefs, Cloud, ...).
    /// TODO : make a service
    /// </summary>
    public abstract class PlayerData
    {
        private static readonly PlayerData Data = new PlayerDataUnity();

        private static int? _currentProfileIndex;

        private static char keySeparator = '#';

        private static EventSystemRef<PlayerDataEventContainer, CoreEventSystem> eventSystem = new EventSystemRef<
            PlayerDataEventContainer, CoreEventSystem>();

        private static readonly string profileIndexKey = "PROFILE_INDEX";
        private static readonly string profilesCount = "PROFILE_COUNT";
        private static readonly string profileNameKey = "PROFILE_NAME";
        private static readonly string profileKeyList = "PROFILE_KEYS";
        private static HashSet<string> _currentProfileKeys;


        // CAUTION!, there could be gaps, this count only the number of profiles,
        // but one can directly create a profile with any index without creating 
        // any lower index profile
        public static int ProfilesCount
        {
            get
            {
                return Data.LoadIntInternal(profilesCount, 0);
            }
            private set
            {
                Data.SaveIntInternal(profilesCount, value);
            }
        }

        public static bool HasProfile(int profileIndex)
        {
            return Data.InternalHasKey(GetKeyForProfile(profileNameKey, profileIndex));
        }

        public static void DeleteProfile(int profileIndex)
        {
            if(!HasProfile(profileIndex))
            {
                Debug.LogWarning("Trying to delete inexisting profile " + profileIndex.ToString());
                return;
            }
            HashSet<string> keys = GetProfileKeys(profileIndex);
            foreach (string key in keys)
            {
                string realKey = GetKeyForProfile(key, profileIndex);
                Data.InternalDelete(realKey);
            }

            string keyList = GetKeyForProfile(profileKeyList, profileIndex);
            Data.InternalDelete(keyList);
            string keyName = GetKeyForProfile(profileNameKey, profileIndex);
            Data.InternalDelete(keyName);
            --ProfilesCount;
            eventSystem.Events.onProfileDeleted.Invoke(profileIndex);
        }


        public static void CreateProfile(int profileIndex, string name)
        {
            if(HasProfile(profileIndex))
            {
                DeleteProfile(profileIndex);
            }
            SetProfileName(profileIndex, name);
            ++ProfilesCount;
            eventSystem.Events.onProfileCreated.Invoke(profileIndex);
        }

        public static int CurrentProfileIndex
        {
            get
            {
                if(!_currentProfileIndex.HasValue)
                {
                   _currentProfileIndex = Data.LoadIntInternal(profileIndexKey, 0);
                   _currentProfileKeys = GetProfileKeys(_currentProfileIndex.Value);
                }
                return _currentProfileIndex.Value;
            }
            set
            {
                if(_currentProfileIndex.HasValue && _currentProfileIndex.Value == value)
                {
                    return;
                }
                Data.SaveIntInternal(profileIndexKey,value);
                _currentProfileIndex = value;
                _currentProfileKeys = GetProfileKeys(_currentProfileIndex.Value);
                eventSystem.Events.onProfileSelected.Invoke(value);
            }
        }

        public static string CurrentProfileName
        {
            get
            {
                return GetProfileName(CurrentProfileIndex);
            }
            set
            {
                SetProfileName(CurrentProfileIndex, value);
            }
        }

        public static string GetProfileName(int profileIndex)
        {
            return Data.LoadStringInternal(GetKeyForProfile(profileNameKey, profileIndex), string.Empty);
        }

        public static void SetProfileName(int profileIndex, string name)
        {
            bool changed = (name != GetProfileName(profileIndex));
            Data.SaveStringInternal(GetKeyForProfile(profileNameKey, profileIndex), name);
            if(changed)
                eventSystem.Events.onProfileRenamed.Invoke(profileIndex);
        }

        private static string GetKeyForProfile(string key, int profileIndex)
        {
            return profileIndex <= 0
                ? key
                : "Profile_" + profileIndex.ToString("00") + key;
        }

        private static string GetKeyForCurrentProfile(string key, bool addToSet)
        {
            var result = GetKeyForProfile(key, CurrentProfileIndex);
            if(addToSet)
            {
                if(_currentProfileKeys.Add(key))
                {
                    SaveCurrentProfileKeys();
                }
            }
            return result;
        }

        private static StringBuilder b = new StringBuilder();

        private static void SaveCurrentProfileKeys()
        {
            b.Length = 0;
            foreach (string key in _currentProfileKeys)
            {
#if DEBUG
                if(key.Any(c => c == keySeparator))
                {
                    throw new Exception("key "+key+"contains forbidden character " + keySeparator.ToString());
                }
#endif
                if(b.Length>0)
                {
                    b.Append(keySeparator);
                }
                b.Append(key);
            }
            Data.SaveStringInternal(GetKeyForCurrentProfile(profileKeyList, false), b.ToString());
        }

        private static string GetRawProfileKeys(int profileIndex)
        {
            string keys = Data.LoadStringInternal(GetKeyForProfile(profileKeyList, profileIndex), "");
            return keys;
        }

        private static HashSet<string> GetProfileKeys(int profileIndex)
        {
            var profileKeys = new HashSet<string>();
            string keys = GetRawProfileKeys(profileIndex);
            if(!string.IsNullOrEmpty(keys))
            {
                foreach (string key in keys.Split(keySeparator))
                {
                    profileKeys.Add(key);
                }
            }
            return profileKeys;
        }

#if CHECK_SAVE
        private static void LogSaveError(string text)
        {
            Debug.LogError(text);
            CheatMessages.Display(text.InRed(), 3600);
        }
#endif

        /// <summary>
        /// Saves the value identified by the key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SaveString(string key, string value)
        {
            string keyForProfile = GetKeyForCurrentProfile(key, true);
            Data.SaveStringInternal(keyForProfile, value);
#if CHECK_SAVE
            string check = Data.LoadStringInternal(keyForProfile, "");
            if(check != value)
            {
                LogSaveError("ERROR CHECKING SAVED STRING FOR " 
                                      + keyForProfile 
                                      + " saved \"" + value +"\" " +
                                      "got \"" + check);
            }
#endif
        }
        public static void SaveInt(string key, int value)
        {
            string keyForProfile = GetKeyForCurrentProfile(key, true);
            Data.SaveIntInternal(keyForProfile, value);
#if CHECK_SAVE
            int check = Data.LoadIntInternal(keyForProfile, 0);
            if (check != value)
            {
                LogSaveError("ERROR CHECKING SAVED INT FOR "
                                      + keyForProfile
                                      + " saved \"" + value + "\" " +
                                      "got \"" + check);
            }
#endif

        }
        public static void SaveFloat(string key, float value)
        {
            string keyForProfile = GetKeyForCurrentProfile(key, true);
            Data.SaveFloatInternal(keyForProfile, value);
#if CHECK_SAVE
            float check = Data.LoadFloatInternal(keyForProfile, 0.0f);
            if (Math.Abs(check - value) > 0.00001f)
            {
                LogSaveError("ERROR CHECKING SAVED FLOAT FOR "
                                      + keyForProfile
                                      + " saved \"" + value + "\" " +
                                      "got \"" + check);
            }
#endif

        }
        
        public static void SaveObject(string key, object value)
        {
            string keyForProfile = GetKeyForCurrentProfile(key, true);
            Data.SaveObjectInternal(keyForProfile, value);
        }

        /// <summary>
        /// Returns the value corresponding to key.
        /// If it doesn't exist, defaultValue is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public static string LoadString(string key, string defaultValue)
        {
            return Data.LoadStringInternal(GetKeyForCurrentProfile(key, false), defaultValue);
        }
        public static int LoadInt(string key, int defaultValue)
        {
            return Data.LoadIntInternal(GetKeyForCurrentProfile(key, false), defaultValue);
        }
        public static float LoadFloat(string key, float defaultValue)
        {
            return Data.LoadFloatInternal(GetKeyForCurrentProfile(key, false), defaultValue);
        }
        public static object LoadObject(string key, object defaultValue)
        {
            return Data.LoadObjectInternal(GetKeyForCurrentProfile(key, false), defaultValue);
        }

        public static void Delete(string key)
        {
            string realKey = GetKeyForCurrentProfile(key, false);
            if(_currentProfileKeys != null)
            {
                _currentProfileKeys.Remove(realKey);
                SaveCurrentProfileKeys();
            }

            Data.InternalDelete(realKey);
        }

        /// <summary>
        /// Checks if the key exists in playerdata 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>        
        public static bool HasKey(string key)
        {
            return Data.InternalHasKey(GetKeyForCurrentProfile(key, false));
        }
        
        protected abstract void SaveStringInternal(string key, string value);
        protected abstract void SaveIntInternal(string key, int value);
        protected abstract void SaveFloatInternal(string key, float value);
        protected abstract void SaveObjectInternal(string key, object value);

        protected abstract string LoadStringInternal(string key, string defaultValue);
        protected abstract int    LoadIntInternal(string key, int defaultValue);
        protected abstract float  LoadFloatInternal(string key, float defaultValue);
        protected abstract object LoadObjectInternal(string key, object defaultValue);

        protected abstract bool InternalHasKey(string key);
        protected abstract void InternalDelete(string key);

        public class ProfileVariable<T> : SavedType<T>
        {
            private int profileIndex;

            public override bool HasValue
            {
                get { return profileIndex == CurrentProfileIndex; }
                protected set { 
                    profileIndex = value ? CurrentProfileIndex : -1; 
                }
            }

            protected ProfileVariable(string key, T defaultValue, SaveDelegate save, LoadDelegate load) : base(key, defaultValue, save, load)
            {
                profileIndex = -1;
                //   variables.Add(new WeakReference(this));
            }

            public override string ToString()
            {
                return GetKeyForProfile(Key, CurrentProfileIndex).InCyan() 
                       + " " 
                       + Value.ToString().InColor(Equals(Value,defaultValue) ? Color.red : Color.green);
            }

            ~ProfileVariable()
            {
                eventSystem.UnregisterFromAllEvents(this);
            }
        }

        public class ProfileDate : ProfileVariable<DateTime>
        {
            public ProfileDate(string key, DateTime defaultValue) : base(key, defaultValue, 
                (s, time) => SaveObject(s, time),
                (s, time) => (DateTime)LoadObject(s, defaultValue))
            {
            }
        }



        public class ProfileBool : ProfileVariable<bool>
        {
            public ProfileBool(string key, bool defaultValue = false)
                : base(key, defaultValue,
                    (s, b) => SaveInt(s, b ? 1 : 0), (s, b) => LoadInt(s, defaultValue ? 1 : 0) == 1)
            {
            }
        }

        public class ProfileInt : ProfileVariable<int>
        {
            public ProfileInt(string key, int defaultValue = 0)
                : base(key, defaultValue, SaveInt, LoadInt)
            {
            }

            public static ProfileInt operator++(ProfileInt p)
            {
                p.Value = p.Value + 1;
                return p;
            }
        }

        public class ProfileFloat : ProfileVariable<float>
        {
            public ProfileFloat(string key, float defaultValue = 0)
                : base(key, defaultValue, SaveFloat, LoadFloat)
            {
            }
        }

        public class ProfileString : ProfileVariable<string>
        {
            public ProfileString(string key, string defaultValue = "")
                : base(key, defaultValue, SaveString, LoadString)
            {
            }
        }


        [CheatFunction(CheatCategory.Data, true)]
        static void ShowProfileKeys()
        {
            const float displayTime = 30;
            int currentProfile = CurrentProfileIndex;
            for(int i = 0; i < 5;++i)
            {
                CheatMessages.Display("Profile " + i 
                                                 + " " + GetProfileName(i).InColor(currentProfile==i?Color.green:Color.green*0.66f) 
                                                 + " = " + GetRawProfileKeys(i),displayTime);
            }
            Debug.Log("Current profile keys".InColor(Color.magenta));
            foreach (string key in _currentProfileKeys)
            {
                Debug.Log(key);
            }
        }

        public static bool NameIsValid(string name, int forProfile, int maxProfileCount)
        {
            if(string.IsNullOrEmpty(name))
            {
                return false;
            }

            for(int i = maxProfileCount;--i>=0;)
            {
                if(i == forProfile)
                    continue;
                string nameI = GetProfileName(i);
                if(nameI == name)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
