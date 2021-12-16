using System;

namespace AbilitySystem.Abilities
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
        public int cost;
    }

    public enum AbilityActivateResult : byte
    {
        Success,
        NotReady,
        NotEnoughResource,
        AlreadyActivate,
        NotFound,
    }
}