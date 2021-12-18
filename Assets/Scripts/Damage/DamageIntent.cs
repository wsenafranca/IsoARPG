using Character;
using UnityEngine;

namespace Damage
{
    public struct DamageIntent
    {
        public CharacterBase source;
        public DamageFlags flags;
        public DamageType damageType;
        public float attackBonus;
        public Vector3 worldPosition;
    }
}