using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using OSG.Core.EventSystem;
using OSG.EventSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OSG
{
    public class Localization : ScriptableObjectSingleton<Localization>
    {
        [EditorPrefs] private static bool dontWarnOnMissingKeys;

        public const string LOCALIZATION_DISABLE_KEY = "localization_disabled_key";
        public const string LOCALIZATION_DEBUG_KEY = "localization_debug_key";

        public enum eLocalizationFilesSource
        {
            streamingAssets,
            assets
        }

        [SerializeField] public SystemLanguage[] supportedLanguages;
        
        public TextColors textColors;

        public eLocalizationFilesSource fileSource;

        public static bool HasAnyKey => Instance._dictionary != null && Instance._dictionary.Keys.Any();

        [ConditionalHideFunction("SourceIsAssets", true)]
        public List<TextAsset> localisationFiles;

        [ConditionalHideFunction("SourceIsStreamingAssets", true)]
        public List<String> localisationFilesPath; //Path of the files in streaming assets

        private List<XmlDocument> _documents;

        EventSystemRef<LocalizationEventContainer, CoreEventSystem> eventSystem = new EventSystemRef<LocalizationEventContainer, CoreEventSystem>();

        public List<XmlDocument> documents
        {
            get
            {
                if (_documents == null)
                {
                    _documents = new List<XmlDocument>();
                }

                if (_documents.Count == 0)
                {
                    if (fileSource == eLocalizationFilesSource.assets && localisationFiles.Count > 0)
                    {
                        foreach (var localisationFile in localisationFiles)
                        {
                            XmlDocument aDocument = new XmlDocument();
                            aDocument.LoadXml(localisationFile.text);
                            _documents.Add(aDocument);
                        }
                    }
                    else if (fileSource == eLocalizationFilesSource.streamingAssets)
                    {
                        if (Application.isPlaying)
                        {
                            //Create a dummy gameobject to start loading coroutine
                            GameObject aGo = new GameObject();
                            CoroutineStarter mono = aGo.AddComponent<CoroutineStarter>();
                            mono.StartCoroutine(LoadFromStreamingAssetCoroutine(aGo));
                            Debug.Log("is playing and streamingAssets");
                        }
                        else
                        {
                            for (var index = 0; index < localisationFilesPath.Count; index++)
                            {
                                var localisationFilePath = localisationFilesPath[index];
                                XmlDocument aDocument = new XmlDocument();
                                string path = Application.streamingAssetsPath + "/" + localisationFilePath;
                                string text = File.ReadAllText(path);
                                Debug.Log("Loading localisation from streaming assets".InColor(Color.red));
                                Debug.Log(path+" starts with "+text.Substring(0, 20));
                                Debug.Log("First character is " + text.ToCharArray()[0]);

                                int indexLess = text.IndexOf("<");
                                if(indexLess>0)
                                {
                                    text = text.Substring(indexLess);
                                }
                                aDocument.LoadXml(text);
                                _documents.Add(aDocument);
                            }

                            LoadLanguage(CurrentLanguage);
                            Debug.Log("Load language in documents getter " + CurrentLanguage);
                            eventSystem.Events.localizationLoaded.Invoke();
                            Debug.Log("GameEventSystem Invoke else isPlaying");
                        }
                    }
                }

                return _documents;
            }
        }

        public bool SourceIsAssets()
        {
            return fileSource == eLocalizationFilesSource.assets;
        }

        public bool SourceIsStreamingAssets()
        {
            return fileSource == eLocalizationFilesSource.streamingAssets;
        }

        IEnumerator LoadFromStreamingAssetCoroutine(GameObject goToDestroy)
        {
            //Use a document array to make sure we keep the files in the right order
            XmlDocument[] documentArray = new XmlDocument[localisationFilesPath.Count];
            for (var index = 0; index < localisationFilesPath.Count; index++)
            {
                var localisationFilePath = localisationFilesPath[index];
                XmlDocument aDocument = new XmlDocument();
                string text = "";
                string path = Application.streamingAssetsPath + "/" + localisationFilePath;
                Debug.Log("Loading "+path);
                if (path.Contains("://"))
                {
//                    UnityEngine.Networking.UnityWebRequest www =
//                        UnityEngine.Networking.UnityWebRequest.Get(path);
//                    yield return www.SendWebRequest();
//
//                    
//                    text = www.downloadHandler.text;
//                    text = text.Trim();
                    WWW www = new WWW(path);
                    while (!www.isDone)
                    {
                        
                    }
                    text = www.text;
                    //Prevent XML error on Android https://stackoverflow.com/questions/17795167/xml-loaddata-data-at-the-root-level-is-invalid-line-1-position-1
                    Debug.Log("Loading localisation from streaming assets".InColor(Color.red));
                    Debug.Log(path+" starts with "+text.Substring(0, 20));
                    Debug.Log("First character is " + text.ToCharArray()[0]);

                }
                else
                {
                    text = File.ReadAllText(path);
                }

                
                Debug.Log("XML Starts with "+text.Substring(0, 200));
                Debug.Log("First character is "+text.ToCharArray()[0]);
                try
                {
                    byte[] bytes = Encoding.Default.GetBytes(text);
                    text = Encoding.UTF8.GetString(bytes);
                    int firstMark = text.IndexOf('<');
                    if(firstMark > 0)
                    {
                        text = text.Substring(firstMark);
                    }
                    aDocument.LoadXml(text);
                    documentArray[index] = aDocument;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error when loading XML : "+e.Message);
                    throw;
                }
            }

            _documents.AddRange(documentArray);
            LoadLanguage(CurrentLanguage);
            Debug.Log("current language in LoadFromStreamingAssetCoroutine " + CurrentLanguage);
            yield return new WaitForEndOfFrame();
            Debug.Log("GameEventSystem Invoke coroutine");
            eventSystem.Events.localizationLoaded.Invoke();
            Destroy(goToDestroy);
        }

#if UNITY_EDITOR
        static SystemLanguage editorLanguage = SystemLanguage.English;
        public static SystemLanguage EditorLanguage
        {
            get
            {
                editorLanguage = (SystemLanguage) (System.Enum.Parse(typeof(SystemLanguage),
                    UnityEditor.EditorPrefs.GetString("EditorLanguage", SystemLanguage.English.ToString())));
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

        static public SystemLanguage SupportedSystemLanguage
        {
            get
            {
                SystemLanguage appLanguage = Application.systemLanguage;
#if UNITY_EDITOR
                appLanguage = (SystemLanguage) UnityEditor.EditorPrefs.GetInt("LANGUAGE", (int) appLanguage);
                appLanguage = EditorLanguage;
#endif
                for (int i = 0; i < Instance.supportedLanguages.Length; i++)
                {
                    if (Instance.supportedLanguages[i] == appLanguage)
                    {
                        Debug.Log("find applanguage in instance supported languages (scriptable objects) : " +
                                  appLanguage);
                        return appLanguage;
                    }
	            }

                if (Instance.supportedLanguages.Length >= 1)
                {
                    Debug.Log("Returning first language : " + Instance.supportedLanguages[0]);
                    return Instance.supportedLanguages[0];
                }
                
                Debug.Log("Returning default language English");
                return SystemLanguage.English;
            }
        }

        [HideInInspector] public SystemLanguage? _currentLanguage = null;

        Dictionary<string, string> _dictionary = new Dictionary<string, string>();


        /// <summary>
        /// Name of the currently active language.
        /// </summary>
        public SystemLanguage CurrentLanguage
        {
            get
            {
                if (_currentLanguage == null)
                {
                    if (PlayerPrefs.HasKey("chosenLanguage"))
                        _currentLanguage = supportedLanguages[PlayerPrefs.GetInt("chosenLanguage")];
                    else
                        _currentLanguage = SupportedSystemLanguage;

                    LoadLanguage(_currentLanguage);
                }
				
				return _currentLanguage.Value;
			}
			set
            {
                if (_currentLanguage != value)
                {
                    Debug.Log("Setting currentLanguage with " + value);
                    _currentLanguage = value;
                    LoadLanguage(value);
                }
            }
        }


#if UNITY_EDITOR
        public void SaveXmlDocument(XmlDocument document = null)
        {
            if (document == null)
                document = documents[0];

            for (int i = 0; i < documents.Count; i++)
            {
                if (documents[i] == document)
                {
                    XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    XmlElement root = document.DocumentElement;
                    try
                    {
                        document.InsertBefore(declaration, root);
                    }
                    catch(Exception e)
                    {
                        // ça sert à quoi cette daube ?
                        Debug.LogWarning(e.Message);
                    }

                    if (SourceIsAssets())
                    {
                        document.Save(AssetDatabase.GetAssetPath(localisationFiles[i]));
                        Debug.Log("Saving in " + AssetDatabase.GetAssetPath(localisationFiles[i]));
                    }
                    else
                    {
                        string path = Application.streamingAssetsPath + "/" + localisationFilesPath[i];
                        document.Save(path);
                        Debug.Log("Saving in " + path);
                    }

                    return;
                }
            }

            Debug.LogWarning("Couldn't save XML document");
        }
#endif

        /// <summary>
        /// Load localization values.
        /// </summary>
        public void LoadLanguage(SystemLanguage? language)
        {
            Debug.Log("Load language on Localization");
            if (language == null)
                return;
            if (_dictionary != null)
                _dictionary.Clear();
            else
                _dictionary = new Dictionary<string, string>();

            //Temporary to have streaming assets working on Android. For debug purpose only
            if (SourceIsStreamingAssets() && (_documents == null || _documents.Count == 0))
            {
                int i = documents.Count; // Do anything to start
                //documents initialisation will call LoadLanguage again
                return;
            }

            foreach (var document in documents)
            {
                XmlNodeList locNodes = document.SelectNodes("localizationData/loc");
                foreach (XmlNode locNode in locNodes)
                {
                    XmlNode idNode = locNode.SelectSingleNode("id");
                    XmlNode valueNode = locNode.SelectSingleNode(language.Value.ToIsoCode());

                    if (idNode != null && !string.IsNullOrEmpty(idNode.InnerText))
                        _dictionary[idNode.InnerText] = (valueNode != null) ? valueNode.InnerText : "";
                }
            }
            eventSystem.Events.localizationChanged.Invoke();
        }

#if UNITY_EDITOR

        private static readonly char[] Trim = {' ','\r','\t','\t'};
        public XmlNode CreateOrModifyXmlNode(string newKey, string valueForCurrentLanguage = "", string status="", XmlDocument document = null)
        {
            if (document == null) document = documents[0];

            //If no characters can be trimmed from the current instance, the method returns the current instance unchanged.
            valueForCurrentLanguage = valueForCurrentLanguage.TrimEnd(Trim);

            if (valueForCurrentLanguage == newKey)
            {
                // it's already been replaced by the key in the asset
                Debug.LogWarning(($"Not touching key {newKey} as it's the same as the given value for text... You should not call this with the key as valueForCurrentLanguage" ).InOrange());
                return GetXmlNode(newKey, document);
            }       

            // Create an XmlNamespaceManager to resolve the default namespace.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            nsmgr.AddNamespace("loc", "urn:loc-schema");
            nsmgr.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");


            XmlNode locNode = GetXmlNode(newKey, document);
            const string statusName = "status";
            if (locNode != null)
            {
                //If the node exists, replace the value of the current language with the one given in parameter
          
                string isoCode = editorLanguage.ToIsoCode();
                var textNode = locNode[isoCode]??locNode.AppendChild(document.CreateElement(isoCode));
                var statusNode = locNode[statusName] ?? locNode.AppendChild(document.CreateElement(statusName));

                if (textNode.InnerText != valueForCurrentLanguage)
                {
                    Debug.Log(("Modifying key " + newKey).InOrange());
                    textNode.InnerText = valueForCurrentLanguage;
                    statusNode.InnerText = StatusModified;
                }
                //for (int i = 0; i < locNode.ChildNodes.Count; i++)
                //{
                //    XmlNode node = locNode.ChildNodes[i];
                //    if (node.Name == editorLanguage.ToIsoCode())
                //    {
                //        node.InnerText = valueForCurrentLanguage;
                //    }
                //    else if (node.Name == "status")
                //    {
                //        node.InnerText = "modified";
                //    }
                //}

                return locNode;
            }

            //XmlNode rootNode = 
            document.SelectSingleNode("localizationData");
            XmlElement locElement = document.CreateElement("loc");
            XmlElement idElement = document.CreateElement("id");
            idElement.InnerText = newKey;
            locElement.AppendChild(idElement);

            Debug.Log(("Adding key " + newKey).InRed());
            
            if (!string.IsNullOrEmpty(status))
            {
                XmlElement statusElement = document.CreateElement(statusName);
                statusElement.InnerText = status;
                locElement.AppendChild(statusElement);
            }
            
            foreach (var language in supportedLanguages)
            {
                XmlElement langElement = document.CreateElement(language.ToIsoCode());
                langElement.InnerText = (language == editorLanguage) ? valueForCurrentLanguage : "";
                locElement.AppendChild(langElement);
            }

            return locElement;
        }

        protected const string StatusNew = "New";
        protected const string StatusModified = StatusNew; // Flavien dixit no "Modifed";

        public XmlNode AddOrModifyLocalizationKey(string newKey, string valueForCurrentLanguage = "",
            XmlDocument document = null)
        {
            XmlNode rootNode = document.SelectSingleNode("localizationData");
            XmlNode locNode = GetXmlNode(newKey, document);
            bool existed = locNode != null;
            XmlNode locElement = CreateOrModifyXmlNode(newKey, valueForCurrentLanguage, StatusNew, document);

            if (!existed)
                rootNode.AppendChild(locElement);

            return locElement;
        }

        public XmlNode InsertLocalizationKeyBefore(string newKey, XmlNode existingNode,
            string valueForCurrentLanguage = "", XmlDocument document = null)
        {
            XmlNode rootNode = document.SelectSingleNode("localizationData");
            XmlNode locElement = CreateOrModifyXmlNode(newKey, valueForCurrentLanguage, StatusNew, document);

            rootNode.InsertBefore(locElement, existingNode);

            return locElement;
        }

        public XmlNode InsertLocalizationKeyAfter(string newKey, XmlNode existingNode,
            string valueForCurrentLanguage = "", XmlDocument document = null)
        {
            XmlNode rootNode = document.SelectSingleNode("localizationData");
            XmlNode locElement = CreateOrModifyXmlNode(newKey, valueForCurrentLanguage, StatusNew, document);

            rootNode.InsertAfter(locElement, existingNode);

            return locElement;
        }
        
        public static XmlNode GetXmlNode(string idKey, XmlDocument document)
        {
            var expression = String.Format("/localizationData/loc[id/text() =\"{0}\"]", idKey);
            var node = document.SelectSingleNode(expression);
            return node;
        }
#endif

        /// <summary>
        /// Localize the specified value.
        /// </summary>
        public static string Localize(string key)
        {
            if (!HasAsset)
            {
                UnityEngine.Debug.LogWarning("Localization instance is null");
                return key;
            }

#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool(LOCALIZATION_DEBUG_KEY, false))
                return "xxx";
            if (UnityEditor.EditorPrefs.GetBool(LOCALIZATION_DISABLE_KEY, false))
                return key;
#endif

            if (key == null)
                return null;

            if (string.IsNullOrEmpty(key))
                return string.Empty;

            
            if (!HasAnyKey)
                Instance.LoadLanguage(Instance.CurrentLanguage);
            string val;
            if (!Instance._dictionary.TryGetValue(key, out val))
            {
                val = key;
            	if(!dontWarnOnMissingKeys)
             		Debug.LogWarning("Localization key not found: '" + key + "'");
            }

            if (Instance.CurrentLanguage == SystemLanguage.Hebrew || Instance.CurrentLanguage == SystemLanguage.Arabic)
            {
                val = RTL.GetText(val, false, false, 0, true);
            }

			val = val.Contains("%ce%") ? ColorizeText(val) : val;
            return val;
        }


        public static bool HasKey(string key)
        {
            return HasAsset && Instance._dictionary.ContainsKey(key);
        }

        public List<string> LocalizationKeys => _dictionary.Keys.ToList();

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
                    "<color=#" + Localization.Instance.textColors.GetRGB(textColor) + ">" + textToColor + "</color>");
            }

            return val;
        }

        public void UpdateLanguageSystems(SystemLanguage[] systemLanguages)
        {
            List<SystemLanguage> temporarySystemLanguages = new List<SystemLanguage>();
            
            for (int i = 0; i < systemLanguages.Length; i++)
            {
                if (supportedLanguages.Contains(systemLanguages[i]))
                {
                    temporarySystemLanguages.Add(systemLanguages[i]);
                }
            }
            
//            supportedLanguages = systemLanguages;
            supportedLanguages = temporarySystemLanguages.ToArray();
        }
    }

    public static class LocalizationHelpers
    {
        public static string ToIsoCode(this SystemLanguage lang)
        {
            string res = "EN";
            switch (lang) {
                case SystemLanguage.Afrikaans: res = "AF"; break;
                case SystemLanguage.Arabic: res = "AR"; break;
                case SystemLanguage.Basque: res = "EU"; break;
                case SystemLanguage.Belarusian: res = "BY"; break;
                case SystemLanguage.Bulgarian: res = "BG"; break;
                case SystemLanguage.Catalan: res = "CA"; break;
                case SystemLanguage.Chinese: res = "ZH"; break;
                case SystemLanguage.ChineseSimplified: res = "CHS"; break;
                case SystemLanguage.ChineseTraditional: res = "CHT"; break;
                case SystemLanguage.Czech: res = "CS"; break;
                case SystemLanguage.Danish: res = "DA"; break;
                case SystemLanguage.Dutch: res = "NL"; break;
                case SystemLanguage.English: res = "EN"; break;
                case SystemLanguage.Estonian: res = "ET"; break;
                case SystemLanguage.Faroese: res = "FO"; break;
                case SystemLanguage.Finnish: res = "FI"; break;
                case SystemLanguage.French: res = "FR"; break;
                case SystemLanguage.German: res = "DE"; break;
                case SystemLanguage.Greek: res = "EL"; break;
                case SystemLanguage.Hebrew: res = "IW"; break;
                case SystemLanguage.Hungarian: res = "HU"; break;
                case SystemLanguage.Icelandic: res = "IS"; break;
                case SystemLanguage.Indonesian: res = "ID"; break;
                case SystemLanguage.Italian: res = "IT"; break;
                case SystemLanguage.Japanese: res = "JP"; break;
                case SystemLanguage.Korean: res = "KR"; break;
                case SystemLanguage.Latvian: res = "LV"; break;
                case SystemLanguage.Lithuanian: res = "LT"; break;
                case SystemLanguage.Norwegian: res = "NO"; break;
                case SystemLanguage.Polish: res = "PL"; break;
                case SystemLanguage.Portuguese: res = "PT"; break;
                case SystemLanguage.Romanian: res = "RO"; break;
                case SystemLanguage.Russian: res = "RU"; break;
                case SystemLanguage.SerboCroatian: res = "SH"; break;
                case SystemLanguage.Slovak: res = "SK"; break;
                case SystemLanguage.Slovenian: res = "SL"; break;
                case SystemLanguage.Spanish: res = "ES"; break;
                case SystemLanguage.Swedish: res = "SV"; break;
                case SystemLanguage.Thai: res = "TH"; break;
                case SystemLanguage.Turkish: res = "TR"; break;
                case SystemLanguage.Ukrainian: res = "UK"; break;
                case SystemLanguage.Unknown: res = "EN"; break;
                case SystemLanguage.Vietnamese: res = "VN"; break;
            }
            return res;
        }
    }
}