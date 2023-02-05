// Old Skull Games
// Bernard Barthelemy
// Wednesday, May 29, 2019

using UnityEngine;

namespace OSG
{
    public class DisableBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.gameObject.SetActive(false);
        }
    }
}
