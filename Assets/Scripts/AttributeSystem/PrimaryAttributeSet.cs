using System;
using UnityEngine;

namespace AttributeSystem
{
    [RequireComponent(typeof(AttributeSet))]
    public class PrimaryAttributeSet : MonoBehaviour
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

        private int currentStrength
        {
            get => strength;
            set
            {
                if (value == strength) return;

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
        
        private int currentStamina
        {
            get => stamina;
            set
            {
                if (value == stamina) return;

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
        
        private int currentDexterity
        {
            get => dexterity;
            set
            {
                if (value == dexterity) return;

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
        
        private int currentIntelligence
        {
            get => intelligence;
            set
            {
                if (value == intelligence) return;

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
    }
}