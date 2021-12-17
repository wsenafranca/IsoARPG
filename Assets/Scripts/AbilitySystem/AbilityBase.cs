using System;
using UnityEngine;

namespace AbilitySystem
{
    public abstract class AbilityBase : ScriptableObject
    {
        public string displayName;
        
        public string displayDescription;
        
        public Texture2D icon;

        public AbilityCost cost = new AbilityCost{resource  = AbilityCostResource.Mana, value = 0};
        
        public float cooldown;
        
        public bool isReady => Time.time - _lastActivatedTime >= cooldown;

        public bool isActive { get; private set; }

        [NonSerialized]
        private float _lastActivatedTime;

        protected AbilityBase()
        {
            isActive = false;
            _lastActivatedTime = 0;
        }

        public AbilityActivateResult CommitAbility(AbilitySystemComponent source)
        {
            if (!isReady) return AbilityActivateResult.NotReady;
            
            if (isActive) return AbilityActivateResult.AlreadyActivate;

            if (source.handler == null || !source.handler.ConsumeAbilityResource(this)) return AbilityActivateResult.NotEnoughResource;
            
            isActive = true;
            _lastActivatedTime = Time.time;
            
            OnActivate(source);
            return AbilityActivateResult.Success;
        }

        public bool Deactivate(AbilitySystemComponent source)
        {
            if (!isActive) return false;
            
            isActive = false;
            OnDeactivate(source);
            return true;
        }
        
        protected abstract void OnActivate(AbilitySystemComponent source);
        protected abstract void OnDeactivate(AbilitySystemComponent source);
    }
}
