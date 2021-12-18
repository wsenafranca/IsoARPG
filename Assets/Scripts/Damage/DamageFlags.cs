using System;

namespace Damage
{
    [Flags]
    public enum DamageFlags
    {
        AlwaysHit,
        IgnoreEnergyShield,
        IgnoreDefense,
        IgnoreElementalResistance,
        IgnoreBlock,
        AbsorbHealth,
        AbsorbMana,
        AbsorbEnergyShield,
    }
}