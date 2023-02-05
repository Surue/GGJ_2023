// Old Skull Games
// Bernard Barthelemy
// Tuesday, August 1, 2017

using UnityEngine;
namespace OSG
{
    /// <summary>
    /// Allows multipicker to detect 
    /// </summary>
    public interface IPickable
    {
        bool IsUnder(Vector2 mousePosition);
    }
}