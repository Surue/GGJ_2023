// Old Skull Games
// Pierre Planeau
// Wednesday, April 10, 2019

using UnityEngine;
using System;

namespace OSG
{
    /// <summary>
    /// Suspends the coroutine execution until the given DateTime is reached.
    /// </summary>
    public class WaitUntilDateTime : CustomYieldInstruction
    {
        protected DateTime endTime;

        public override bool keepWaiting
        {
            get
            {
                return DateTime.UtcNow < endTime;
            }
        }

        public WaitUntilDateTime(DateTime dateTime)
        {
            endTime = dateTime;
        }
    }
}
