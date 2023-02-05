// Old Skull Games
// Bernard Barthelemy
// Friday, June 29, 2018
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Save and load data from the PlayerPrefs.
    /// </summary>
    public class PlayerDataUnity : PlayerData
    {
        protected override void SaveStringInternal(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        protected override void SaveIntInternal(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        protected override void SaveFloatInternal(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        protected override void SaveObjectInternal(string key, object value)
        {
            PlayerPrefsSerializer.Save(key, value);
            PlayerPrefs.Save();
        }


        protected override string LoadStringInternal(string key, string defaultValue)
        {
            string value = PlayerPrefs.GetString(key, defaultValue);
            return value;
        }

        protected override int LoadIntInternal(string key, int defaultValue)
        {
            int value = PlayerPrefs.GetInt(key, defaultValue);
            return value;
        }

        protected override float LoadFloatInternal(string key, float defaultValue)
        {
            float value = PlayerPrefs.GetFloat(key, defaultValue);
            return value;
        }

        protected override object LoadObjectInternal(string key, object defaultValue)
        {
            return PlayerPrefsSerializer.Load(key) ?? defaultValue;
        }

        protected override bool InternalHasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        protected override void InternalDelete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }

}