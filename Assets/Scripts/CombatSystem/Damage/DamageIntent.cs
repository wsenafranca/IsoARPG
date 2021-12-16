using UnityEngine;

namespace CombatSystem.Damage
{
    public enum DamageTarget
    {
        Health,
        Mana,
        MagicShield
    }
    
    public struct DamageIntent
    {
        public GameObject source;
        public DamageFlags flags;
        public DamageTarget damageTarget;
    }
}