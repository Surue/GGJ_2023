// Old Skull Games
// Antoine Pastor
// 24/04/2018

#if TM_PRO
using OSG;
using OSG.Core;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedFontOverrider : OSGMono {

	// Use this for initialization
	void Start ()
	{
		
		TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
		if (text)
		{
			if (LocalizedFontList.Instance)
			{
				text.font = LocalizedFontList.Instance.FontForLanguage(Localization.Instance.CurrentLanguage, text.font);
			}
			else
			{
				Debug.LogWarning("You need a LocalizedFontList to use LocalizedFontOverrider");
			}
			
			using(var context = GameEventSystem.Context(this))
			{
				eventContainer events = context;
				events.localizationLoaded.AddListener(OnLocalizationLoaded);
			}
		}
		
	}

	void OnLocalizationLoaded()
	{
		if (LocalizedFontList.Instance)
		{
			TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
			if (text)
			{
				text.font = LocalizedFontList.Instance.FontForLanguage(Localization.Instance.CurrentLanguage, text.font);
			}
		}
	}
}
#endif
