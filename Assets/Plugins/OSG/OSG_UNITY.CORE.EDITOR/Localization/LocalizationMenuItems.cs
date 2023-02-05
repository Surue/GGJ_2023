// Old Skull Games
// antoinepastor  
// Wednesday, 27 September 2017

#if TM_PRO
using TMPro;
#endif

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OSG
{
    public static class LocalizationMenuItems
    {
        
        const string SelectScriptable = "OSG/Localization/Select scriptable";
        const string DisableMode = "OSG/Localization/Disable in editor";
        const string DebugMode = "OSG/Localization/Debug mode (xxx)";
        
        private static bool currentEditorLocalizeValue;


        public static void ToggleLanguage(SystemLanguage language)
        {
            UnityEditor.EditorPrefs.SetBool(Localization.LOCALIZATION_DEBUG_KEY, false);
            UnityEditor.EditorPrefs.SetBool(Localization.LOCALIZATION_DISABLE_KEY, false);
            Localization.EditorLanguage = language;
            Localization.Instance.LoadLanguage(language);
        }

        public static bool ValidateFunction(SystemLanguage language, string menuPath)
        {
            Menu.SetChecked(menuPath, Localization.EditorLanguage == language && DebugModesDisabled());
            return true;
        }
        
        // Disable
        [MenuItem(SelectScriptable)]
        public static void SelectScriptableObject()
        {
            ScriptableObjectUtility.FocusInInspector(typeof(Localization));
        }


        // Disable
        [MenuItem(DisableMode)]
        public static void ToggleDisable()
        {
            UnityEditor.EditorPrefs.SetBool(Localization.LOCALIZATION_DEBUG_KEY, false);
            UnityEditor.EditorPrefs.SetBool(Localization.LOCALIZATION_DISABLE_KEY, true);
          
        }
        [MenuItem(DisableMode, true)]
        public static bool ToggleDisableValidate ()
        {
            Menu.SetChecked(DisableMode, EditorPrefs.GetBool(Localization.LOCALIZATION_DISABLE_KEY));
            return true;
        }
        
        [MenuItem(DebugMode)]
        public static void ToggleDebug()
        {
            UnityEditor.EditorPrefs.SetBool(Localization.LOCALIZATION_DISABLE_KEY, false);
            UnityEditor.EditorPrefs.SetBool(Localization.LOCALIZATION_DEBUG_KEY, true);
          
        }
        [MenuItem(DebugMode, true)]
        public static bool ToggleDebugValidate ()
        {
            Menu.SetChecked(DebugMode, EditorPrefs.GetBool(Localization.LOCALIZATION_DEBUG_KEY));
            return true;
        }


        public static bool DebugModesDisabled()
        {
            return (EditorPrefs.GetBool(Localization.LOCALIZATION_DEBUG_KEY) == false &&
                    EditorPrefs.GetBool(Localization.LOCALIZATION_DISABLE_KEY) == false);
        }
               
    
        //Localize a text on a gameobject
        [MenuItem("OSG/Localization/Localize selected %e")]
        public static void LocalizeSelected()
        {
            GameObject selectedGO = Selection.activeGameObject;

            if (selectedGO)
            {
                InputStringDialog.Open(
                    (key) => LocalizeGameObject(selectedGO, key),
                    "Choose a key",
                    true);    
            }
            else
            {
                Debug.LogWarning("You must select a Gameobject to localize");
            }   
        }
        
        //Localize a text on a gameobject
        [MenuItem("OSG/Localization/Set localization key")]
        public static void SetLocalizationKey()
        {
            GameObject selectedGO = Selection.activeGameObject;
            if (selectedGO)
            {
                InputStringDialog.Open(
                    (key) => AddTextLocalizerWithKey(selectedGO, key),
                    "Choose a key",
                    true);    
            }
            else
            {
                Debug.LogWarning("You must select a Gameobject to localize");
            }   
        }

        [MenuItem("OSG/Localization/Reload current localization")]
        public static void ReloadCurrent(){
            Localization.Instance.LoadLanguage(Localization.Instance.CurrentLanguage);    
        }
        
        [MenuItem("OSG/Localization/Clear")]
        public static void ClearDocuments(){
            Localization.Instance.documents.Clear();
        }
        
        private static bool AddTextLocalizerWithKey(GameObject goToLocalize, string key)
        {
            
            if (
                #if TM_PRO 
                goToLocalize.GetComponent<TextMeshProUGUI>() == null && 
                #endif
                goToLocalize.GetComponent<Text>() == null)
            {
                Debug.LogWarning("The GameObject you Localize must have a TextMeshProUGUI or a Text component");
                return false;
            }

            if (goToLocalize.GetComponent<TextLocalizer>())
            {
                Debug.LogWarning("The GameObject already has a TextLocalizer component");
                return true;
            }

            TextLocalizer loc = goToLocalize.AddComponent<TextLocalizer>();
            loc.localizationId = key;
            return true;
        }
        
        private static bool LocalizeGameObject(GameObject goToLocalize, string key)
        {
            AddTextLocalizerWithKey(goToLocalize, key);
            string textValue = "";
#if TM_PRO
            if (goToLocalize.GetComponent<TextMeshProUGUI>())
                textValue = goToLocalize.GetComponent<TextMeshProUGUI>().text;
            else 
            #endif
            if (goToLocalize.GetComponent<Text>())
                textValue = goToLocalize.GetComponent<Text>().text;
            
            Localization.Instance.AddOrModifyLocalizationKey(key, textValue);
            Localization.Instance.SaveXmlDocument();
            
            return true;
        }

    }
}