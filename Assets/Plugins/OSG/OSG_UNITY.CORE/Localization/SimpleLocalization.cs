using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Localization manager is able to parse localization information from text assets.
    /// Although a singleton, you will generally not access this class as such. Instead
    /// you should implement "void Localize (Localization loc)" functions in your classes.
    /// </summary>
    ///

    [ExecuteInEditMode]
    public class SimpleLocalization : MonoBehaviour
    {
#if UNITY_EDITOR
        static SystemLanguage editorLanguage = SystemLanguage.English;
        public static SystemLanguage EditorLanguage
        {
            get
            {
                editorLanguage = (SystemLanguage)(System.Enum.Parse(typeof(SystemLanguage), UnityEditor.EditorPrefs.GetString("EditorLanguage", SystemLanguage.English.ToString())));
                return editorLanguage;
            }
            set
            {
                if (value != editorLanguage)
                {
                    editorLanguage = value;
                    UnityEditor.EditorPrefs.SetString("EditorLanguage", value.ToString());
                }
            }
        }
#endif

        [SerializeField]
        SystemLanguage[] supportedLanguages;
        public TextColors textColors;

        public static event Action OnLanguageChanged;

        public TextAsset localisationFile;

        static SimpleLocalization _instance;

        /// <summary>
        /// The instance of the localization class. Will create it if one isn't already around.
        /// </summary>
        static public SimpleLocalization instance
        {
            get
            {
                return _instance;
            }
        }

        static public SystemLanguage SupportedSystemLanguage
        {
            get
            {
                SystemLanguage appLanguage = Application.systemLanguage;
#if UNITY_EDITOR
                appLanguage = (SystemLanguage) UnityEditor.EditorPrefs.GetInt("LANGUAGE", (int)appLanguage);
                appLanguage = SimpleLocalization.EditorLanguage;
#endif

                for (int i = 0; i < _instance.supportedLanguages.Length; i++)
                    if (_instance.supportedLanguages[i] == appLanguage)
                        return appLanguage;

                return SystemLanguage.English;
            }
        }

        SystemLanguage _currentLanguage;

        Dictionary<string, string> _dictionary;



        /// <summary>
        /// Name of the currently active language.
        /// </summary>
        public SystemLanguage CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                if (_currentLanguage != value)
                {

                    _currentLanguage = value;
                    LoadLanguage(value);
                    UnityEngine.Debug.Log("Current language : " + _currentLanguage.ToString());
                    //BroadcastMessage("Localize", SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        void Awake()
        {
            if (Application.isPlaying)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(_instance);
                }
                _instance = this;
                DontDestroyOnLoad(gameObject);
                _currentLanguage = SupportedSystemLanguage;
                LoadLanguage(_currentLanguage);
            }
        }

        /// <summary>
        /// Remove the instance reference.
        /// </summary>
        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// Load localization values.
        /// </summary>
        void LoadLanguage(SystemLanguage language)
        {
            if (localisationFile != null)
            {
                if (_dictionary != null)
                    _dictionary.Clear();
                else
                    _dictionary = new Dictionary<string, string>();

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(localisationFile.text);
                XmlNodeList locNodes = xmlDocument.SelectNodes("localizationData/localization");
                foreach (XmlNode locNode in locNodes)
                {
                    XmlNode idNode = locNode.SelectSingleNode("id");
                    XmlNode valueNode = locNode.SelectSingleNode(language.ToString());

                    if (idNode != null && valueNode != null &&
                        !string.IsNullOrEmpty(idNode.InnerText) && !string.IsNullOrEmpty(valueNode.InnerText))
                        _dictionary[idNode.InnerText] = valueNode.InnerText;
                }
            }
            else
                _dictionary = null;

            if (OnLanguageChanged != null)
                OnLanguageChanged();
        }

        public static void UpdateLocalizationFile(TextAsset textAsset)
        {
            instance.localisationFile = textAsset;
            instance.LoadLanguage(instance._currentLanguage);
        }

        /// <summary>
        /// Localize the specified value.
        /// </summary>
        public static string Localize(string key)
        {
            if (instance == null)
            {
                UnityEngine.Debug.LogWarning("Localization instance is null");
                return key;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && !UnityEditor.EditorPrefs.GetBool("sfw_localize_in_editor", false))
                return key;
#endif

            if (key == null)
                return null;

            string val = null;

            if (instance._dictionary == null)
                instance.LoadLanguage(instance.CurrentLanguage);

            if (instance._dictionary.ContainsKey(key))
            {
                val = instance._dictionary[key];
            }
            else
            {
                UnityEngine.Debug.LogWarning("Localization key not found: '" + key + "'");
                val = key;
            }

            if (instance.CurrentLanguage == SystemLanguage.Hebrew 
             || instance.CurrentLanguage == SystemLanguage.Arabic)
            {
                val = RTL.GetText(val, false, false, 0, true);
            }

            val = val.Contains("%ce%") ? ColorizeText(val) : val;
            return val;
        }

        public List<string> LocalizationKeys
        {
            get
            {
                return _dictionary.Keys.ToList();
            }
        }

        private static string ColorizeText(string val)
        {
            string pattern = @"%c:([a-z]*?)%(.+?)%ce%";
            MatchCollection elements = Regex.Matches(val, pattern);

            int elementLenght = elements.Count;
            for (var i = 0; i < elementLenght; i++)
            {
                Match element = elements[i];
                string textToReplace = element.Groups[0].Value;
                string textColor = element.Groups[1].Value;
                string textToColor = element.Groups[2].Value;

                val = val.Replace(textToReplace,
                    "<color=#" + SimpleLocalization.instance.textColors.GetRGB(textColor) + ">" + textToColor + "</color>");
            }
            return val;
        }

    }
}
