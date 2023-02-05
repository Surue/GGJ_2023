// Old Skull Games
// Pierre Planeau
// Tuesday, January 16, 2018

using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Suspends the coroutine execution for the given amount of milliseconds using scaled time.
    /// </summary>
    public class WaitForMilliseconds : CustomYieldInstruction
    {
        protected int endTime;

        public override bool keepWaiting => TimeExtensions.timeMilli < endTime;

        public void Set(int milliseconds)
        {
            endTime = TimeExtensions.timeMilli + milliseconds;
        }

        public WaitForMilliseconds(int milliseconds)
        {
            Set(milliseconds);
        }
    }
}
