using System;
using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Abilities;
using UnityEngine;

namespace AbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    { 
        public AttributeSet attributeSet;
        
        [SerializeField]
        private List<AbilityBase> abilities;

        [SerializeField] private Transform damageLocation;
        
        private readonly List<EffectData> _effects = new();
        private readonly Queue<DamageInfo> _damages = new();

        private EffectAggregator[] _effectAggregators;

        public event Action<DamageInfo> OnReceivedDamage;
        public event Action<DamageInfo> OnEnergyShieldBreak;
        
        public event Action<IEffectSource, float> OnEffectAdd;
        public event Action<IEffectSource, float, int> OnEffectStack;
        public event Action<IEffectSource> OnEffectApplied;
        public event Action<IEffectSource> OnEffectRemove;
        
        public event Action<AbilityBase, float> OnAbilityActivated;
        public event Action<AbilityBase> OnAbilityDeactivated;
        
        public bool isAnyAbilityActive => abilities.Count((ability) => ability.isActive) > 0;

        private void Awake()
        {
            _effectAggregators = new EffectAggregator[AttributeSet.numAttributes];
            for (var i = 0; i < _effectAggregators.Length; i++)
            {
                _effectAggregators[i] = new EffectAggregator();
            }
        }

        private void Start()
        {
            attributeSet.Init();
            attributeSet.Restore();
        }

        private void LateUpdate()
        {
            _effects.RemoveAll(OnApplyEffect);

            for (var attribute = 0; attribute < AttributeSet.numAttributes; attribute++)
            {
                _effectAggregators[attribute].CalculateAttributeValue(attributeSet, (Attribute)attribute);
            }

            while (_damages.TryDequeue(out var damage))
            {
                OnApplyDamage(damage);
            }
        }
        
        public void AddEffect(IEffectSource effect)
        {
            var data = _effects.Find((data) => data.effect == effect);
            
            if (data == null)
            {
                data = new EffectData(effect);
                _effects.Add(data);
                
                effect.OnAddEffect(this);
                OnEffectAdd?.Invoke(effect, data.creationTime);
            }
            else if(data.AddStack())
            {
                effect.OnAddStack(this, data.stack);
                OnEffectStack?.Invoke(effect, data.creationTime, data.stack);
            }
        }

        private bool OnApplyEffect(EffectData data)
        {
            if (data.IsEffectReady())
            {
                data.effect.OnApplyEffect(this);
                OnEffectApplied?.Invoke(data.effect);
                return false;
            }

            if (data.isValid) return false;
            
            RemoveEffect(data.effect);
            return true;
        }

        public void RemoveEffect(IEffectSource effect)
        {
            foreach (var aggregator in _effectAggregators)
            {
                aggregator.RemoveEffect(effect);
            }
            
            effect.OnRemoveEffect(this);
            OnEffectRemove?.Invoke(effect);
        }

        public void ApplyEffectModifier(IEffectSource effect, EffectModifier modifier, Attribute attribute, float value)
        {
            _effectAggregators[(int)attribute].AddEffect(effect, modifier, value);;
        }

        public void ApplyAdditiveEffect(IEffectSource effect, Attribute attribute, float value) =>
            ApplyEffectModifier(effect, EffectModifier.Additive, attribute, value);
        
        public void ApplyMultiplicativeEffect(IEffectSource effect, Attribute attribute, float scale) => 
            ApplyEffectModifier(effect, EffectModifier.Multiplicative, attribute, scale);

        public void ApplyOverrideEffect(IEffectSource effect, Attribute attribute, float value) => 
            ApplyEffectModifier(effect, EffectModifier.Override, attribute, value);
        
        public int IndexOfAbility<T>() where T : AbilityBase
        {
            return abilities.FindIndex((ability) => ability.GetType() == typeof(T));
        }
        
        public void AddAbility(AbilityBase ability)
        {
            if (abilities.Contains(ability)) return;
            
            abilities.Add(ability);
        }

        public void RemoveAbility(AbilityBase ability)
        {
            abilities.Remove(ability);
        }

        public void RemoveAbility<T>() where T : AbilityBase
        {
            abilities.RemoveAll((ability) => ability.GetType() == typeof(T));
        }
        
        public bool IsActiveAbility(int index)
        {
            return index >= 0 && abilities[index].isActive;
        }

        public AbilityActivateResult TryActivateAbility<T>() where T : AbilityBase
        {
            return TryActivateAbility(IndexOfAbility<T>());
        }
        
        public AbilityActivateResult TryActivateAbility(int index)
        {
            if (index < 0) return AbilityActivateResult.NotFound;

            var ret = abilities[index].CommitAbility(this);
            if (ret == AbilityActivateResult.Success)
            {
                OnAbilityActivated?.Invoke(abilities[index], abilities[index].cooldown);
            }
            
            return ret;
        }
        
        public void DeactivateAbility<T>() where T : AbilityBase
        {
            DeactivateAbility(IndexOfAbility<T>());
        }

        public void DeactivateAbility(int index)
        {
            if (index < 0 || !abilities[index].Deactivate(this)) return;
            
            OnAbilityDeactivated?.Invoke(abilities[index]);
        }

        public void DeactivateAllAbilities()
        {
            foreach (var ability in abilities.Where(ability => ability.isActive))
            {
                ability.Deactivate(this);
            }
        }

        public void ApplyDamage(DamageIntent intent)
        {
            ApplyDamage(DamageExecution.CalculateDamage(intent, this));
            foreach (var effect in intent.effects)
            {
                AddEffect(effect);
            }
        }

        public void ApplyDamage(DamageInfo damage)
        {
            _damages.Enqueue(damage);
        }
        
        private void OnApplyDamage(DamageInfo damage)
        {
            if (damage.isBlocked)
            {
                DamageOutputManager.instance.ShowText(damageLocation.position, "Block", Color.white);
                return;
            }
            
            if (damage.isMiss)
            {
                DamageOutputManager.instance.ShowText(damageLocation.position, "Miss", Color.gray);
                return;
            }

            if (damage.isCritical)
            {
                DamageOutputManager.instance.ShowText(damageLocation.position, "Critical", Color.yellow);
            }
            
            float appliedDamage;
            switch (damage.damageTarget)
            {
                case DamageTarget.Health:
                case DamageTarget.MagicShield:
                    if (attributeSet.GetMagicShield() > 0 && !damage.ignoreMagicShield && damage.damage > 0)
                    {
                        damage.damageTarget = DamageTarget.MagicShield;
                        appliedDamage = attributeSet.GetMagicShield() - damage.damage;
                        attributeSet.SetMagicShield(appliedDamage);
                        DamageOutputManager.instance.ShowDamage(damageLocation.position, damage);
                        
                        if (appliedDamage <= 0)
                        {
                            DamageOutputManager.instance.ShowText(damageLocation.position, "Break", Color.cyan);
                            OnEnergyShieldBreak?.Invoke(damage);
                        
                            // shield was broken and the rest of damage will be sent back to health
                            damage.damage += appliedDamage;
                        }
                    }
                    if (attributeSet.GetMagicShield() <= 0 && damage.damage != 0)
                    {
                        damage.damageTarget = DamageTarget.Health;
                        appliedDamage = attributeSet.GetHealth() - damage.damage;
                        attributeSet.SetHealth(appliedDamage);
                        DamageOutputManager.instance.ShowDamage(damageLocation.position, damage);
                    }
                    break;
                case DamageTarget.Mana:
                    appliedDamage = attributeSet.GetMana() - damage.damage;
                    attributeSet.SetMana(appliedDamage);
                    DamageOutputManager.instance.ShowDamage(damageLocation.position, damage);
                    break;
            }
            
            OnReceivedDamage?.Invoke(damage);
        }
    }
}