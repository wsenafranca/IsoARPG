using UnityEngine;

namespace Character
{
    public class AnimationLocker : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterAnimator = animator.GetComponent<CharacterAnimator>();
            if(characterAnimator) characterAnimator.LockAnimation();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var characterAnimator = animator.GetComponent<CharacterAnimator>();
            if(characterAnimator) characterAnimator.UnlockAnimation();
        }
    }
}