using UnityEngine;

namespace AbilitySystem.Effects
{
    public abstract class EffectBase : ScriptableObject, IEffectSource
    {
        public string displayName;
        
        public string displayDescription;
        
        public Texture2D icon;
        
        public float duration = 1;
        
        public float period = 0.1f;

        public int maxStack;

        public bool resetTimeOnStack;
        
        public abstract void OnAddEffect(AbilitySystemComponent target);

        public abstract void OnAddStack(AbilitySystemComponent target, int stack);

        public abstract void OnApplyEffect(AbilitySystemComponent target);

        public abstract void OnRemoveEffect(AbilitySystemComponent target);

        public float GetPeriod() => period;

        public float GetDuration() => duration;

        public int GetMaxStack() => maxStack;

        public bool GetResetTimeOnStack() => resetTimeOnStack;
    }
}