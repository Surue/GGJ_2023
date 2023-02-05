using UnityEngine;

namespace OSG
{
    public static class ColorExtensions { 

        public static Color SetChan(this Color vColor, char cChan, float fVal)
        {
            switch (cChan)
            {
                case 'r': vColor.r = fVal; break;
                case 'g': vColor.g = fVal; break;
                case 'b': vColor.b = fVal; break;
                case 'a': vColor.a = fVal; break;
            }
		
            return vColor;
        }

        /// <summary>
        /// Returns the specified RGB color with alpha value.
        /// </summary>
        public static Color Alpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Returns the specified RGB color with relative alpha value.
        /// </summary>
        public static Color AlphaRelative(this Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }

        /// <summary>
        /// Returns the hexadecimal representation of the Color32.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToHex(this Color32 color)
        {
            return string.Format("0x{0}{1}{2}", color.r.ToString("X2"), color.g.ToString("X2"), color.b.ToString("X2"));
        }

        /// <summary>
        /// Returns the hexadecimal representation of the Color.
        /// Look at ColorUtility.ToHtmlStringRGB(Color c) for an html representation of the Color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToHex(this Color color)
        {
            return ((Color32)color).ToHex();
        }

        public static float GetChan(this Color vColor, char cChan)
        {
            switch (cChan)
            {
                case 'r': return vColor.r;
                case 'g': return vColor.g;
                case 'b': return vColor.b;
                case 'a': return vColor.a;
                default: return 0;
            }
        }
    }
}