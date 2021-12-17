using System;

namespace AbilitySystem
{
    public enum AbilityCostResource
    {
        Health,
        Mana,
        EnergyShield,
        Gold
    }
    
    [Serializable]
    public struct AbilityCost
    {
        public AbilityCostResource resource;
        public int value;
    }

    public enum AbilityActivateResult : byte
    {
        Success,
        NotReady,
        NotEnoughResource,
        AlreadyActivate,
        NotFound,
        CannotUseAbility
    }
}