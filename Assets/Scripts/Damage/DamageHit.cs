using UnityEngine;

namespace Damage
{
    public struct DamageHit
    {
        public int value;
        public DamageFlags flags;
        public DamageType damageType;
        public bool criticalHit;
        public bool missedHit;
        public bool blockedHit;
        public Vector3 worldPosition;
        public Vector3 normal;
    }
}