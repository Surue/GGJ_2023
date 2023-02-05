// Old Skull Games
// Flavien Caston
// Monday, July 24, 2018

using OSG;
using UnityEngine;

public class MultiPlatformTextLocalizer : TextLocalizer {

	[SerializeField, LocalizedText]
	public string pcLocalizationId;

	protected override string GetLocalizationId()
	{
		if (Input.touchSupported)
			return localizationId;
		else
			return pcLocalizationId;
	}
}
