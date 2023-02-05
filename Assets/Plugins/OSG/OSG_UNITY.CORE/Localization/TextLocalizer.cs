
#if TM_PRO
using TMPro;
#endif

using OSG.Core.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace OSG
{
    [DisallowMultipleComponent]
    public class TextLocalizer : MonoBehaviour
    {
        [SerializeField, LocalizedText]
		public string localizationId;

	    protected Text text;

        EventSystemRef<LocalizationEventContainer, CoreEventSystem> eventSystem = new EventSystemRef<
            LocalizationEventContainer, CoreEventSystem>();
        
        #if TM_PRO
        protected TMP_Text textMeshProText;
        #endif
        
        void Awake()
        {
            text = GetComponent<Text>();
            
            #if TM_PRO
            if (!text)
                textMeshProText = GetComponent<TMP_Text>();


            if (LocalizedFontList.HasAsset)
            {
                if (LocalizedFontList.Instance.FontForLanguage(Localization.Instance.CurrentLanguage, textMeshProText.font) != textMeshProText.font)
                    gameObject.AddComponent<LocalizedFontOverrider>();
            }
			#endif
            
            using(var events = eventSystem.RegisterContext(this))
            {
                events.localizationLoaded.AddListener(OnLanguageChanged);
                events.localizationChanged.AddListener(OnLanguageChanged);
            }
        }

        void OnEnable()
        {
            Localize();
        }

        protected virtual void Localize()
        {
            if (!Application.isPlaying)
                return;
           
            string value = Localization.Localize(GetLocalizationId());
            if (text)
            {
                text.text = value;
        
                if (text.fontStyle == FontStyle.Italic)
                    text.fontStyle = FontStyle.BoldAndItalic;    
            }
            #if TM_PRO
            else if (textMeshProText)
            {
                textMeshProText.SetText(value);
            }
            #endif
        }

        protected virtual string GetLocalizationId()
        {
            return localizationId;
        }

        void OnLanguageChanged ()
        {
            Debug.Log("LanguageChanged in text localizer");
            Localize();
        }
	}
}