using System;
using AttributeSystem;
using InventorySystem;
using Item;
using Weapon;

namespace Player
{
    public class PlayerInventoryController : InventoryController, IWeaponMeleeHandler
    {
        private PlayerAnimator _animator;
        private AttributeSet _attributeSet;
        private readonly WeaponMelee[] _weaponMelee = new WeaponMelee[2];

        protected override void Awake()
        {
            base.Awake();
            _attributeSet = GetComponent<AttributeSet>();
            _animator = GetComponent<PlayerAnimator>();
        }

        private void OnDisable()
        {
            _weaponMelee[0] = _weaponMelee[1] = null;
        }

        public WeaponMelee GetWeaponMelee(int index)
        {
            return _weaponMelee[index];
        }

        public override bool Equip(EquipmentSlot slot, IInventoryEquipmentItem item)
        {
            if (!base.Equip(slot, item)) return false;

            if (item is not EquipmentItemInstance equipment) return false;

            foreach (var attribute in equipment.attributes)
            {
                if (_attributeSet.TryGetAttribute(attribute.attribute, out var value))
                {
                    value.AddModifier(new AdditiveAttributeModifier(attribute.value.currentValue, equipment));
                }
            }
            
            foreach (var modifier in equipment.additiveModifiers)
            {
                if (_attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.AddModifier(new AdditiveAttributeModifier(modifier.value, equipment));
                }
            }
            
            foreach (var modifier in equipment.multiplicativeModifiers)
            {
                if (_attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.AddModifier(new MultiplicativeAttributeModifier(modifier.value, equipment));
                }
            }
            
            switch (slot)
            {
                case EquipmentSlot.MainHand:
                {
                    TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[0] = itemObj.GetComponent<WeaponMelee>();
                    break;
                }
                case EquipmentSlot.OffHand:
                {
                    TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[1] = itemObj.GetComponent<WeaponMelee>();
                    break;
                }
                case EquipmentSlot.Helm:
                case EquipmentSlot.Armor:
                case EquipmentSlot.Pants:
                case EquipmentSlot.Gloves:
                case EquipmentSlot.Boots:
                case EquipmentSlot.Necklace:
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                case EquipmentSlot.Cape:
                case EquipmentSlot.Pet:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
            
            UpdateEquipmentAnimation(slot); 
            
            return true;
        }

        public override bool UnEquip(EquipmentSlot slot, out IInventoryEquipmentItem item)
        {
            if (!base.UnEquip(slot, out item)) return false;
            
            if (item is not EquipmentItemInstance equipment) return false;
            
            foreach (var attribute in equipment.attributes)
            {
                if (_attributeSet.TryGetAttribute(attribute.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            foreach (var modifier in equipment.additiveModifiers)
            {
                if (_attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            foreach (var modifier in equipment.multiplicativeModifiers)
            {
                if (_attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            switch (slot)
            {
                case EquipmentSlot.MainHand:
                {
                    TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[0] = null;
                    break;
                }
                case EquipmentSlot.OffHand:
                {
                    TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[1] = null;
                    break;
                }
                case EquipmentSlot.Helm:
                case EquipmentSlot.Armor:
                case EquipmentSlot.Pants:
                case EquipmentSlot.Gloves:
                case EquipmentSlot.Boots:
                case EquipmentSlot.Necklace:
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                case EquipmentSlot.Cape:
                case EquipmentSlot.Pet:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
            
            UpdateEquipmentAnimation(slot);
            
            return true;
        }

        private void UpdateEquipmentAnimation(EquipmentSlot slot)
        {
            var mainHand = GetEquipmentItem(EquipmentSlot.MainHand);
            var offHand = GetEquipmentItem(EquipmentSlot.OffHand);

            if (mainHand is {equipmentType:EquipmentType.OneHandWeapon} && offHand is not { equipmentType: EquipmentType.Shield })
            {
                _animator.EquipOneHandSword();
            }
            else if (offHand is { equipmentType: EquipmentType.Shield })
            {
                _animator.EquipSwordAndShield();
            }
            else if (mainHand is { equipmentType: EquipmentType.TwoHandWeapon })
            {
                _animator.EquipTwoHandSword();
            }
            else
            {
                _animator.UnEquip();
            }
        }
    }
}