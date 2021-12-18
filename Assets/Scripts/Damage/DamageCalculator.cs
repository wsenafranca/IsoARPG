using AttributeSystem;
using Character;
using UnityEngine;

namespace Damage
{
    public static class DamageCalculator
    {
        public static DamageInfo CalculateDamage(DamageIntent intent, CharacterBase target) => CalculateDamage(intent.source, target, intent.flags, intent.damageType, intent.attackBonus, intent.worldPosition);
        
        public static DamageInfo CalculateDamage(CharacterBase source, CharacterBase target, DamageFlags damageFlags, DamageType damageType, float attackBonus, Vector3 worldPosition)
        {
            var outDamage = new DamageInfo
            {
                flags = damageFlags,
                damageType = damageType,
                worldPosition =  worldPosition
            };

            if (source == null || target == null) return outDamage;
            
            var hitRate = source.attributeSet.GetAttributeValueOrDefault(Attribute.HitRate) / 100.0f;
            if (hitRate > 0 && Random.value > hitRate && !damageFlags.HasFlag(DamageFlags.AlwaysHit))
            {
                outDamage.missedHit = true;
                return outDamage;
            }
            
            var evasionRate = target.attributeSet.GetAttributeValueOrDefault(Attribute.EvasionRate) / 100.0f;
            if (evasionRate > 0 && Random.value < evasionRate && !damageFlags.HasFlag(DamageFlags.AlwaysHit))
            {
                outDamage.missedHit = true;
                return outDamage;
            }
            
            var blockRate = target.attributeSet.GetAttributeValueOrDefault(Attribute.BlockRate) / 100.0f;
            if (blockRate > 0 && Random.value < blockRate && !damageFlags.HasFlag(DamageFlags.IgnoreBlock))
            {
                outDamage.blockedHit = true;
                return outDamage;
            }
            
            var attackPower = source.attributeSet.GetAttributeValueOrDefault(Attribute.AttackPower) + Random.Range(
                source.attributeSet.GetAttributeValueOrDefault(Attribute.MinAttackPower), source.attributeSet.GetAttributeValueOrDefault(Attribute.MaxAttackPower));

            var defensePower = damageFlags.HasFlag(DamageFlags.IgnoreDefense) ? 0 : target.attributeSet.GetAttributeValueOrDefault(Attribute.DefensePower);

            outDamage.value = 1 + Mathf.FloorToInt((attackPower * 1 + attackBonus) - defensePower);

            if (Random.value < source.attributeSet.GetAttributeValueOrDefault(Attribute.CriticalHitRate) / 100.0f)
            {
                outDamage.criticalHit = true;
                outDamage.value *= 1 + source.attributeSet.GetAttributeValueOrDefault(Attribute.CriticalHitDamage);
            }

            return outDamage;
        }
    }
}