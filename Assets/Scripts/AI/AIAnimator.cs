using Character;
using UnityEngine;

namespace AI
{
    public class AIAnimator : CharacterAnimator
    {
        private static readonly int ChasingID = Animator.StringToHash("chasing");
        
        public bool chasing
        {
            set => animator.SetBool(ChasingID, value);
            get => animator.GetBool(ChasingID);
        }
    }
}