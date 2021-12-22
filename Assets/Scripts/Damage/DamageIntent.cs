using Character;
using SkillSystem;
using UnityEngine;

namespace Damage
{
    public struct DamageIntent
    {
        public CharacterBase instigator;
        public GameObject causer;
        public DamageFlags damageFlags;
        public DamageTargetType damageTargetType;
        public DamageType damageType;
        public SkillInstance skill;
        public Vector3 worldPosition;
        public Vector3 normal;
    }
}