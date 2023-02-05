// Old Skull Games
// Antoine Pastor
// 29/05/2018

using System.Linq;
using System.Xml;
using OSG;
using UnityEditor;

[CustomEditor(typeof(Localization))]
public class LocalizationEditor : CustomEditorBase
{

	private int languageIndex;

	private new void OnEnable()
	{
		base.OnEnable();

		if (EditorPrefs.GetBool(Localization.LOCALIZATION_DISABLE_KEY))
		{
			languageIndex = 0;
		}
		else if (EditorPrefs.GetBool(Localization.LOCALIZATION_DEBUG_KEY))
		{
			languageIndex = 1;
		}
		else if(Localization.Instance)
		{
			for (int i = 0; i < Localization.Instance.supportedLanguages.Length; i++)
			{
				if (Localization.Instance.supportedLanguages[i] == Localization.Instance.CurrentLanguage)
				{
					languageIndex = i + 2; //+2 because index 0 is for disabled and 1 is for xxx
					break;
				}
			}	
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		string[] languageOptions = {"disable", "xxx"};
		int oldIndex = languageIndex;
        if(!Localization.Instance)
        {
            EditorGUILayout.HelpBox($"No Localization Instance loaded", MessageType.Error);
            return;
        }
		languageOptions = languageOptions.Concat(Localization.Instance.supportedLanguages.Select(t=>t.ToString())).ToArray();
		languageIndex = EditorGUILayout.Popup("Editor language", languageIndex, languageOptions);

		if (languageIndex != oldIndex)
		{
			SelectLanguageAtIndex(languageIndex);
		}
		
		EditorGUILayout.LabelField(Localization.Instance.LocalizationKeys.Count+" keys loaded"); 
		
	}

	private static void SelectLanguageAtIndex(int index)
	{
		switch (index)
		{
			case 0:
				LocalizationMenuItems.ToggleDisable();
				break;
			case 1:
				LocalizationMenuItems.ToggleDebug();
				break;
			default:
				LocalizationMenuItems.ToggleLanguage(Localization.Instance.supportedLanguages[index - 2]);
				break;
		}
	}
}
