using UnityEngine;

namespace Damage
{
    public struct DamageInfo
    {
        public int value;
        public DamageFlags flags;
        public DamageType damageType;
        public bool criticalHit;
        public bool missedHit;
        public bool blockedHit;
        public Vector3 worldPosition;
    }
}