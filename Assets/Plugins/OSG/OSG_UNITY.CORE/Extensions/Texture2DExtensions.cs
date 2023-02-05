using UnityEngine;

namespace OSG
{
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Retrives the texture, tinted with the given color.
        /// </summary>
        public static Texture2D Colored(this Texture2D texture, Color color)
        {
            Texture2D coloredTexture = new Texture2D(texture.width, texture.height);
            Color[] pixels = texture.GetPixels();

            int count = pixels.Length;
            for (int p = 0; p < count; p++)
            {
                pixels[p] *= color;
            }

            coloredTexture.SetPixels(pixels);
            coloredTexture.Apply();

            return coloredTexture;
        }

        /// <summary>
        /// Retrives the texture, tinted in red.
        /// </summary>
        public static Texture2D Red(this Texture2D texture)
        {
            return texture.Colored(Color.red);
        }

        /// <summary>
        /// Retrives the texture, tinted in grren.
        /// </summary>
        public static Texture2D Green(this Texture2D texture)
        {
            return texture.Colored(Color.green);
        }

        /// <summary>
        /// Retrives the texture, tinted in blue.
        /// </summary>
        public static Texture2D Blue(this Texture2D texture)
        {
            return texture.Colored(Color.blue);
        }
    }
}