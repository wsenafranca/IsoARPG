using System;
using Character;
using Damage;

namespace SkillSystem
{
    public class SkillInstance
    {
        public Skill skillBase;

        private DateTime _useTime;

        public double remainingCooldown => Math.Max(0.0, skillBase.cooldown - (DateTime.Now - _useTime).TotalSeconds);

        public bool isReady => (DateTime.Now - _useTime).Seconds > skillBase.cooldown;

        public bool IsTargetValid(CharacterBase source, CharacterBase target)
        {
            if (source == null || !source.isAlive || target == null || !target.isAlive) return false;

            return skillBase.damageTargetType switch
            {
                DamageTargetType.Self => source == target,
                DamageTargetType.Opponent => source.IsOpponent(target),
                DamageTargetType.Partner => source.IsPartner(target),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public bool TryUseSkill(CharacterBase character, out DamageIntent damageIntent)
        {
            damageIntent = new DamageIntent();
            if (!CanUseSkill(character)) return false;

            character.currentHealth -= skillBase.healthCost;
            character.currentMana -= skillBase.manaCost;

            _useTime = DateTime.Now;

            damageIntent.instigator = character;
            damageIntent.damageTargetType = skillBase.damageTargetType;
            damageIntent.damageType = skillBase.damageType;
            damageIntent.skill = this;
            damageIntent.damageFlags = skillBase.damageFlags;

            return true;
        }

        public bool CanUseSkill(CharacterBase character)
        {
            return isReady && character.currentHealth >= skillBase.healthCost && character.currentMana >= skillBase.manaCost;
        }
    }
}