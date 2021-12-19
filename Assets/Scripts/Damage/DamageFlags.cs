using System;

namespace Damage
{
    [Flags]
    public enum DamageFlags
    {
        None,
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