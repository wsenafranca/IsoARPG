using System;

namespace CombatSystem.Damage
{
    [Flags]
    public enum DamageFlags
    {
        IgnoreEnergyShield = 0,
        IgnoreDefense = 2,
        IgnoreBlock = 4,
        IgnoreEvasion = 8,
        IgnoreElementalResistance = 16,
        AbsorbHealth = 32,
        AbsorbMana = 64,
        AbsorbEnergyShield = 128,
    }
}