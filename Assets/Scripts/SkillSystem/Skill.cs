using Damage;
using UnityEngine;

namespace SkillSystem
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "SkillSystem/Skill", order = 0)]
    public class Skill : ScriptableObject
    {
        public string displayName;
        public string description;
        public Texture2D icon;

        public SkillRequirements requirements;
        public DamageTargetType damageTargetType;
        public DamageType damageType;
        public float skillDamageBonus;
        public float range;
        public bool needTarget;
        public DamageFlags damageFlags;
        public string animatorTrigger;
        public int healthCost;
        public int manaCost;
        public float cooldown;

        public SkillInstance CreateSkillInstance()
        {
            return new SkillInstance
            {
                skillBase = this
            };
        }
    }
}