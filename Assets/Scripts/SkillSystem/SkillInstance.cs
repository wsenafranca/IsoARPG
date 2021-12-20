using Character;
using Damage;
using UnityEngine;

namespace SkillSystem
{
    public class SkillInstance
    {
        public Skill skillBase;

        private float _useTime;

        public float remainingCooldown => Mathf.Max(0, skillBase.cooldown - (Time.time - _useTime));

        public bool isReady => Time.time - _useTime > skillBase.cooldown;

        public bool TryUseSkill(CharacterBase character, out DamageIntent damageIntent)
        {
            damageIntent = new DamageIntent();
            if (!CanUseSkill(character)) return false;

            character.currentHealth -= skillBase.healthCost;
            character.currentMana -= skillBase.manaCost;

            _useTime = Time.time;

            damageIntent.source = character;
            damageIntent.damageTargetType = skillBase.damageTargetType;
            damageIntent.damageType = skillBase.damageType;
            damageIntent.skillDamageBonus = skillBase.skillDamageBonus;
            damageIntent.damageFlags = skillBase.damageFlags;

            return true;
        }

        public bool CanUseSkill(CharacterBase character)
        {
            return isReady && character.currentHealth >= skillBase.healthCost && character.currentMana >= skillBase.manaCost;
        }

        public bool CanUseSkillAtTarget(CharacterBase character, CharacterBase target)
        {
            if (!CanUseSkill(character) || target == null || !target.isAlive) return false;

            return Vector3.Distance(character.transform.position, target.transform.position) <= skillBase.range;
        }
    }
}