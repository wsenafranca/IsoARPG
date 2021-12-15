using System;
using AbilitySystem.Effects;

namespace AbilitySystem.Abilities
{
    public enum AbilityCostResource
    {
        Health,
        Mana,
        MagicShield,
        Gold
    }
    
    [Serializable]
    public struct AbilityCost
    {
        public AbilityCostResource resource;
        public int cost;
    }

    [Serializable]
    public struct AbilityEffect
    {
        public EffectBase effect;
        public float probability;
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