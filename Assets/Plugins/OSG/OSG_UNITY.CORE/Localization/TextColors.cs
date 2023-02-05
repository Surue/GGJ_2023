// Old Skull Games
// Bernard Barthelemy
// Friday, May 19, 2017

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSG
{
    public class TextColors : ScriptableObject
    {
        [Serializable]
        public struct ColorName
        {
            public string Name;
            public Color Color;
        }

        public List<ColorName> colorNames = new List<ColorName>();

        public string GetRGB(string name)
        {
            foreach (var colorName in colorNames)
            {
                if (colorName.Name == name)
                {
                    return ColorUtility.ToHtmlStringRGBA(colorName.Color);
                }
            }
            return "FFFFFF";
        }


    }
}
