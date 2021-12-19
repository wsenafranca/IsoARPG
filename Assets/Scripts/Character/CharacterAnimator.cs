using UnityEngine;

namespace Character
{
    public class CharacterAnimator : MonoBehaviour
    {
        protected Animator animator;
        private bool _lockAnimation;
        
        private static readonly int SpeedID = Animator.StringToHash("speed");
        private static readonly int MovingID = Animator.StringToHash("moving");
        private static readonly int AnimSpeedID = Animator.StringToHash("animSpeed");

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public float speed
        {
            set => animator.SetFloat(SpeedID, value);
            get => animator.GetFloat(SpeedID);
        }

        public bool moving
        {
            set => animator.SetBool(MovingID, value);
            get => animator.GetBool(MovingID);
        }

        public float animSpeed
        {
            set => animator.SetFloat(AnimSpeedID, value);
            get => animator.GetFloat(AnimSpeedID);
        }

        public bool isPlayingAnimation => _lockAnimation;
        
        public void LockAnimation()
        {
            _lockAnimation = true;
        }

        public void UnlockAnimation()
        {
            _lockAnimation = false;
        }
        
        private void FootR()
        {
            
        }

        private void FootL()
        {
            
        }
    }
}