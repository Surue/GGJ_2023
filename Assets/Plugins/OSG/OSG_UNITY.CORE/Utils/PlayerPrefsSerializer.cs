// Old Skull Games
// Bernard Barthelemy
// Thursday, June 1, 2017

using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OSG
{
    /// <summary>
    /// Serializes an object into a PlayerPref's string
    /// </summary>
    public class PlayerPrefsSerializer
    {
        public static BinaryFormatter bf = new BinaryFormatter();

        // serializableObject is any struct or class marked with [Serializable]
        public static void Save(string prefKey, object serializableObject)
        {
            //Variable d'environement pour les ghosts sur mobile
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

            MemoryStream memoryStream = new MemoryStream();
            bf.Serialize(memoryStream, serializableObject);
            string tmp = Convert.ToBase64String(memoryStream.ToArray());
            PlayerPrefs.SetString(prefKey, tmp);
        }

        public static object Load(string prefKey)
        {
            //Variable d'environement pour les ghosts sur mobile
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

            string tmp = PlayerPrefs.GetString(prefKey, string.Empty);
            if (tmp == string.Empty)
                return null;
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(tmp));
            return bf.Deserialize(memoryStream);
        }
    }
}