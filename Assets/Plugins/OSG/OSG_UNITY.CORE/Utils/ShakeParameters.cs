using OSG.Core;
using UnityEngine;

namespace OSG
{
    public class ShakeParameters : ScriptableObject
    {
        [SerializeField] public int duration;
        [SerializeField] public AnimationCurve verticalMove;
        [SerializeField] public MinMaxRange verticalRange;
        [SerializeField] public AnimationCurve horizontalMove;
        [SerializeField] public MinMaxRange horizontalRange;
        [SerializeField] public int delayBeforeRepeat;
    }
}