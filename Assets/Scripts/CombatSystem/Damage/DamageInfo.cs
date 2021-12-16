namespace CombatSystem.Damage
{
    public struct DamageInfo
    {
        public int value;
        public DamageFlags flags;
        public DamageTarget damageTarget;
        public bool criticalHit;
        public bool missedHit;
        public bool blockedHit;
    }
}