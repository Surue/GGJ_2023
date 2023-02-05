// Old Skull Games
// Pierre Planeau
// Tuesday, January 16, 2018

using UnityEngine;

namespace OSG
{
    public static class TimeExtensions
    {
        /// <summary>
        /// The time at the beginning of the frame (Read-Only).
        /// This is the time in milliseconds since the start of the game.
        /// </summary>
        public static int timeMilli
        {
            get
            {
                return (int)(Time.time * 1000f);
            }
        }
    }
}
