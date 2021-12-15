namespace CombatSystem.Damage
{
    public struct DamageInfo
    {
        public float value;
        public DamageFlags flags;
        public bool criticalHit;
        public bool missedHit;
        public bool blockedHit;
    }
}