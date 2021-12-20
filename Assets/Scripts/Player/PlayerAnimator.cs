using Character;
using UnityEngine;

namespace Player
{
    public class PlayerAnimator : CharacterAnimator
    {
        private static readonly int EquipOneHandSwordID = Animator.StringToHash("EquipOneHandSword");
        private static readonly int EquipSwordAndShieldID = Animator.StringToHash("EquipSwordAndShield");
        private static readonly int EquipTwoHandSwordID = Animator.StringToHash("EquipTwoHandSword");
        private static readonly int UnEquipID = Animator.StringToHash("UnEquip");
        private static readonly int WeaponIndexID = Animator.StringToHash("weaponIndex");
        private static readonly int HitNumberID = Animator.StringToHash("hitNumber");

        protected override void Awake()
        {
            base.Awake();
        }

        public void EquipOneHandSword()
        {
            animator.SetTrigger(EquipOneHandSwordID);
        }
        
        public void EquipSwordAndShield()
        {
            animator.SetTrigger(EquipSwordAndShieldID);
        }

        public void EquipTwoHandSword()
        {
            animator.SetTrigger(EquipTwoHandSwordID);
        }

        public void UnEquip()
        {
            animator.SetTrigger(UnEquipID);
        }
        
        public int weaponIndex
        {
            set => animator.SetInteger(WeaponIndexID, value);
            get => animator.GetInteger(WeaponIndexID);
        }

        public int hitHumber
        {
            set => animator.SetInteger(HitNumberID, value);
            get => animator.GetInteger(HitNumberID);
        }
    }
}