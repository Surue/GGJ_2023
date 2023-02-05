// Old Skull Games
// Pierre Planeau
// Monday, September 25, 2017

using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Use this PropertyAttribute to display an error message in the inspector when the property value is null.
    /// </summary>
    public class NotNullAttribute : PropertyAttribute
    {
        public readonly string message;

        public NotNullAttribute(string message = "Missing object reference for property.")
        {
            this.message = message;
        }
    }
}
