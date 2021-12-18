using Character;
using Controller;
using UnityEngine;

namespace Damage
{
    public struct DamageIntent
    {
        public BaseCharacterController source;
        public DamageFlags flags;
        public DamageType damageType;
        public float attackBonus;
        public Vector3 worldPosition;
    }
}