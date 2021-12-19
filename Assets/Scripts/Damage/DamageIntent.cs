using Character;
using UnityEngine;

namespace Damage
{
    public struct DamageIntent
    {
        public CharacterBase source;
        public DamageFlags damageFlags;
        public DamageTargetType damageTargetType;
        public DamageType damageType;
        public float skillDamageBonus;
        public Vector3 worldPosition;
    }
}