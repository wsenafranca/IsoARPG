using System;
using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Effects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AbilitySystem.Abilities
{
    public abstract class AbilityBase : ScriptableObject
    {
        public string displayName;
        
        public string displayDescription;
        
        public Texture2D icon;

        public AbilityCost cost = new AbilityCost{resource  = AbilityCostResource.Mana, cost = 0};
        
        public float cooldown;

        public AbilityEffect[] effects;

        public bool isReady => Time.time - _lastActivatedTime >= cooldown;

        public bool isActive { get; private set; }

        [NonSerialized]
        private float _lastActivatedTime;

        public AbilityActivateResult CommitAbility(AbilitySystemComponent source)
        {
            if (!isReady) return AbilityActivateResult.NotReady;

            var value = cost.resource switch
            {
                AbilityCostResource.Health => source.attributeSet.GetHealth(),
                AbilityCostResource.Mana => source.attributeSet.GetMana(),
                AbilityCostResource.MagicShield => source.attributeSet.GetMagicShield(),
                AbilityCostResource.Gold => 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (value < cost.cost) return AbilityActivateResult.NotEnoughResource;
            
            if (isActive) return AbilityActivateResult.AlreadyActivate;
            
            isActive = true;
            _lastActivatedTime = Time.time;

            switch (cost.resource)
            {
                case AbilityCostResource.Health:
                    source.attributeSet.SetHealth(source.attributeSet.GetHealth() - cost.cost);
                    break;
                case AbilityCostResource.Mana:
                    source.attributeSet.SetMana(source.attributeSet.GetMana() - cost.cost);
                    break;
                case AbilityCostResource.MagicShield:
                    source.attributeSet.SetMagicShield(source.attributeSet.GetMagicShield() - cost.cost);
                    break;
                case AbilityCostResource.Gold:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
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

        protected List<IEffectSource> GetAvailableEffects()
        {
            return (from data in effects where Random.value < data.probability select data.effect).Cast<IEffectSource>().ToList();
        }
        
        protected abstract void OnActivate(AbilitySystemComponent source);
        protected abstract void OnDeactivate(AbilitySystemComponent source);
    }
}
