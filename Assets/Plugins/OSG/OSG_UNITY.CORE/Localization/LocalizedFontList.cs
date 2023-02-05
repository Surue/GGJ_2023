// Old Skull Games
// Antoine Pastor
// 23/04/2018

#if TM_PRO
using System;
using System.Collections.Generic;
using OSG;
using TMPro;
using UnityEngine;

public class LocalizedFontList : ScriptableObjectSingleton<LocalizedFontList>
{
    [Serializable]
    public struct FontAndLoc
    {
        public SystemLanguage language;
        public TMP_FontAsset fontAsset;
    }

    [Serializable]
    public struct FontOverride
    {
        public TMP_FontAsset originalFont;
        public List<FontAndLoc> overrideFont;
    }

    [SerializeField] private List<FontOverride> fontOverrides;

    public TMP_FontAsset FontForLanguage(SystemLanguage language, TMP_FontAsset originalFont)
    {
        foreach (var anOverride in fontOverrides)
        {
            if (anOverride.originalFont == originalFont)
            {
                foreach (var overrideFont in anOverride.overrideFont)
                {
                    if (overrideFont.language == language)
                        return overrideFont.fontAsset;
                }
            }
        }
        return originalFont;
    }
}
#endif