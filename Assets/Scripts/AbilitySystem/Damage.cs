using System.Collections.Generic;
using AbilitySystem.Effects;
using UnityEngine;

namespace AbilitySystem
{
    public enum DamageTarget
    {
        Health,
        Mana,
        MagicShield
    }
    
    public struct DamageInfo
    {
        public DamageTarget damageTarget;
        public float damage;
        public bool isCritical;
        public bool isBlocked;
        public bool isMiss;
        public bool ignoreMagicShield;
    }

    public struct DamageIntent
    {
        public DamageTarget damageTarget;
        public AbilitySystemComponent source;
        public float damageBonus;
        public List<IEffectSource> effects;
    }

    public static class DamageExecution
    {
        public static DamageInfo CalculateDamage(DamageIntent intent, AbilitySystemComponent target)
        {
            var damageInfo = new DamageInfo()
            {
                damageTarget = intent.damageTarget, 
                damage = 0, 
                isMiss = false,
                isBlocked = false, 
                isCritical = false
            };
            
            if (!intent.source) return damageInfo;

            var evasionRate = target.attributeSet[Attribute.EvasionRate].GetCurrentValue();
            if (Random.value < evasionRate)
            {
                damageInfo.isMiss = true;
                return damageInfo;
            }
            
            var blockRate = target.attributeSet[Attribute.BlockRate].GetCurrentValue();
            if (Random.value < blockRate)
            {
                damageInfo.isBlocked = true;
                return damageInfo;
            }

            var minAttack = intent.source.attributeSet[Attribute.MinAttackPower].GetCurrentValue();
            var maxAttack = intent.source.attributeSet[Attribute.MaxAttackPower].GetCurrentValue();
            
            var damage = Random.Range(minAttack, maxAttack) * intent.damageBonus;
            
            var defenseRate = target.attributeSet[Attribute.DefensePower].GetCurrentValue();
            damage = Mathf.Max(1, damage - defenseRate);
            
            var criticalRate = intent.source.attributeSet[Attribute.CriticalHitRate].GetCurrentValue();
            if (Random.value < Mathf.Clamp01(criticalRate))
            {
                var criticalDamage = intent.source.attributeSet[Attribute.CriticalHitDamage].GetCurrentValue();
                damage *= 2 + criticalDamage;
                damageInfo.isCritical = true;
            }

            damageInfo.damage = Mathf.Max(1, Mathf.CeilToInt(damage));

            return damageInfo;
        }
    }
}