// Old Skull Games
// Bernard Barthelemy
// Friday, November 17, 2017

using UnityEngine;

namespace OSG
{
    public static class AnimatorExtensions
    {
        public static bool HasProperty(this Animator animator, string name)
        {
            return animator.HasProperty(Animator.StringToHash(name));
        }
        
        public static bool HasProperty(this Animator animator, int nameHash)
        {
            if(!animator.runtimeAnimatorController)
                return false;
            if(animator.parameterCount==0)
                return false;
            
            var pars = animator.parameters;
            foreach (AnimatorControllerParameter parameter in pars)
            {
                if(parameter.nameHash == nameHash)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Sets the trigger only if the Animator has it.
        /// Does not generate Warnings in the console.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="nameHash"></param>
        public static void QuietSetTrigger(this Animator animator, int nameHash)
        {
            if (animator.HasProperty(nameHash))
                animator.SetTrigger(nameHash);
        }

        /// <summary>
        /// Resets the trigger only if the Animator has it.
        /// Does not generate Warnings in the console.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="nameHash"></param>
        public static void QuietResetTrigger(this Animator animator, int nameHash)
        {
            if (animator.HasProperty(nameHash))
                animator.ResetTrigger(nameHash);
        }
    }
}