// Old Skull Games
// Bernard Barthelemy
// Monday, February 11, 2019

using UnityEngine;

namespace OSG
{
   public class Colorizer : ScriptableObject
    {
        [SerializeField] private Texture2D grayScale;
        [SerializeField] private Texture2D colorized;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private Material comparisonMaterial;
        [SerializeField] private Material colorizeMaterial;
        [SerializeField] private Color color;
    }
}