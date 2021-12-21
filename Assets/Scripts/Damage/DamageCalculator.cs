using AttributeSystem;
using Character;
using UnityEngine;

namespace Damage
{
    public static class DamageCalculator
    {
        public static DamageHit CalculateDamage(DamageIntent intent, CharacterBase target) => CalculateDamage(intent.source, target, intent.damageFlags, intent.damageType, intent.skillDamageBonus, intent.worldPosition, intent.normal);
        
        public static DamageHit CalculateDamage(CharacterBase source, CharacterBase target, DamageFlags damageFlags, DamageType damageType, float skillDamage, Vector3 worldPosition, Vector3 normal)
        {
            var outDamage = new DamageHit
            {
                flags = damageFlags,
                damageType = damageType,
                worldPosition =  worldPosition,
                normal = normal
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

            outDamage.value = Mathf.Max(1, Mathf.FloorToInt(attackPower * (1 + skillDamage) - defensePower));

            if (Random.value < source.attributeSet.GetAttributeValueOrDefault(Attribute.CriticalHitRate) / 100.0f)
            {
                outDamage.criticalHit = true;
                outDamage.value *= 1 + source.attributeSet.GetAttributeValueOrDefault(Attribute.CriticalHitDamage);
            }

            return outDamage;
        }
    }
}