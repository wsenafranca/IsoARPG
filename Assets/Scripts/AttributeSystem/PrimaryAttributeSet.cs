using System;
using InventorySystem;
using UnityEngine;

namespace AttributeSystem
{
    [RequireComponent(typeof(AttributeSet))]
    public class PrimaryAttributeSet : MonoBehaviour, IInventoryRequirementsSource
    {
        [SerializeField]
        private int strength;
        
        [SerializeField]
        private int stamina;
        
        [SerializeField]
        private int dexterity;
        
        [SerializeField]
        private int intelligence;

        private AttributeSet _attributeSet;

        [HideInInspector]
        public int currentLevel;
        
        public int currentStrength
        {
            get => strength;
            set
            {
                strength = value;
                if (_attributeSet.TryGetAttribute(Attribute.MinAttackPower, out var minAttack))
                {
                    minAttack.RemoveAllModifier(this);
                    minAttack.AddModifier(new AdditiveAttributeModifier(2 + strength / 5, this));
                }
                
                if (_attributeSet.TryGetAttribute(Attribute.MaxAttackPower, out var maxAttack))
                {
                    maxAttack.RemoveAllModifier(this);
                    maxAttack.AddModifier(new AdditiveAttributeModifier(5 + strength / 2, this));
                }
            }
        }
        
        public int currentStamina
        {
            get => stamina;
            set
            {
                stamina = value;
                if (_attributeSet.TryGetAttribute(Attribute.MaxHealth, out var maxHealth))
                {
                    maxHealth.RemoveAllModifier(this);
                    maxHealth.AddModifier(new AdditiveAttributeModifier(20 + stamina * 3, this));
                }
                if (_attributeSet.TryGetAttribute(Attribute.DefensePower, out var defense))
                {
                    defense.RemoveAllModifier(this);
                    defense.AddModifier(new AdditiveAttributeModifier(7 + stamina / 3, this));
                }
                if (_attributeSet.TryGetAttribute(Attribute.BlockRate, out var block))
                {
                    block.RemoveAllModifier(this);
                    block.AddModifier(new AdditiveAttributeModifier(stamina / 32, this));
                }
            }
        }
        
        public int currentDexterity
        {
            get => dexterity;
            set
            {
                dexterity = value;
                if (_attributeSet.TryGetAttribute(Attribute.AttackSpeed, out var attackSpeed))
                {
                    attackSpeed.RemoveAllModifier(this);
                    attackSpeed.AddModifier(new AdditiveAttributeModifier(13 + dexterity * 2 / 7, this));
                }
                if (_attributeSet.TryGetAttribute(Attribute.HitRate, out var hitRate))
                {
                    hitRate.RemoveAllModifier(this);
                    hitRate.AddModifier(new AdditiveAttributeModifier(dexterity / 13, this));
                }
                if (_attributeSet.TryGetAttribute(Attribute.EvasionRate, out var evasion))
                {
                    evasion.RemoveAllModifier(this);
                    evasion.AddModifier(new AdditiveAttributeModifier(dexterity / 50, this));
                }
            }
        }
        
        public int currentIntelligence
        {
            get => intelligence;
            set
            {
                intelligence = value;
                if (_attributeSet.TryGetAttribute(Attribute.MaxMana, out var maxMana))
                {
                    maxMana.RemoveAllModifier(this);
                    maxMana.AddModifier(new AdditiveAttributeModifier(30 + intelligence * 4, this));
                }
                if (_attributeSet.TryGetAttribute(Attribute.SkillDamage, out var skillDamage))
                {
                    skillDamage.RemoveAllModifier(this);
                    skillDamage.AddModifier(new AdditiveAttributeModifier(4 + intelligence / 3, this));
                }
            }
        }
        
        private void Awake()
        {
            _attributeSet = GetComponent<AttributeSet>();
            currentStrength = strength;
            currentStamina = stamina;
            currentDexterity = dexterity;
            currentIntelligence = intelligence;
        }

        public int GetRequirementsValue(EquipmentRequirement requirement)
        {
            return requirement switch
            {
                EquipmentRequirement.Level => currentLevel,
                EquipmentRequirement.Strength => currentStrength,
                EquipmentRequirement.Stamina => currentStamina,
                EquipmentRequirement.Dexterity => currentDexterity,
                EquipmentRequirement.Intelligence => currentIntelligence,
                _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, null)
            };
        }
    }
}