using Character;
using UnityEngine;

namespace Player
{
    public class PlayerAnimator : CharacterAnimator
    {
        private PlayerInventoryController _inventory;
        
        private static readonly int EquipOneHandSwordID = Animator.StringToHash("EquipOneHandSword");
        private static readonly int EquipSwordAndShieldID = Animator.StringToHash("EquipSwordAndShield");
        private static readonly int EquipTwoHandSwordID = Animator.StringToHash("EquipTwoHandSword");
        private static readonly int UnEquipID = Animator.StringToHash("UnEquip");
        private static readonly int NormalAttackID = Animator.StringToHash("NormalAttack");
        private static readonly int WeaponIndexID = Animator.StringToHash("weaponIndex");
        private static readonly int HitNumberID = Animator.StringToHash("hitNumber");

        protected override void Awake()
        {
            base.Awake();
            _inventory = GetComponent<PlayerInventoryController>();
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
        
        public bool PlayNormalAttackAnimation()
        {
            if (isPlayingAnimation) return false;
            
            LockAnimation();
            animator.SetTrigger(NormalAttackID);
            return true;
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
        
        private void OnWeaponBeginAttack(int index)
        {
            if(_inventory) _inventory.GetWeaponMelee(index)?.BeginAttack(gameObject);
        }

        private void OnWeaponEndAttack(int index)
        {
            if(_inventory) _inventory.GetWeaponMelee(index)?.EndAttack();
        }
    }
}