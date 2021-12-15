using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    public interface IEffectSource
    {
        public const int InfinityDuration = -1;
        public const int NoPeriodic = -1;

        public void OnAddEffect(AbilitySystemComponent target);
        
        public void OnAddStack(AbilitySystemComponent target, int stack);
        
        public void OnApplyEffect(AbilitySystemComponent target);
        
        public void OnRemoveEffect(AbilitySystemComponent target);

        public float GetPeriod();

        public float GetDuration();
        
        public int GetMaxStack();

        public bool GetResetTimeOnStack();
    }

    public class EffectData
    {
        public IEffectSource effect { get; private set; }
        public float creationTime { get; private set; }
        public float periodicElapsedTime { get; private set; } = 0;
        public int stack { get; private set; } = 0;
        private bool _didApplyOnce = false;
        
        public EffectData(IEffectSource effect)
        {
            this.effect = effect;
            creationTime = Time.time;
        }

        public bool isValid => effect.GetDuration() <= IEffectSource.InfinityDuration ||
                               Time.time - creationTime < effect.GetDuration();

        public bool hasPeriod => effect.GetPeriod() > IEffectSource.NoPeriodic;

        public bool canStack => stack < effect.GetMaxStack();

        public bool AddStack()
        {
            if (!canStack) return false;

            stack++;

            if (effect.GetResetTimeOnStack()) creationTime = Time.time;
            
            return true;
        }

        public bool IsEffectReady()
        {
            if (!_didApplyOnce)
            {
                _didApplyOnce = true;
                return true;
            }

            if (!hasPeriod) return false;
            
            periodicElapsedTime += Time.deltaTime;
            if (periodicElapsedTime < effect.GetPeriod()) return false;
            
            periodicElapsedTime = 0;
            return true;
        }
    }
    
    public enum EffectModifier
    {
        Additive,
        Multiplicative,
        Override
    }

    public struct EffectAggregatorData
    {
        public IEffectSource effect;
        public EffectModifier modifier;
        public float value;
    }

    public class EffectAggregator
    {
        private bool _isDirty = true;
        private readonly List<EffectAggregatorData> _data = new();

        public void AddEffect(IEffectSource effect, EffectModifier modifier, float value)
        {
            _isDirty = true;
            _data.Add(new EffectAggregatorData
            {
                effect =  effect,
                modifier =  modifier, 
                value = value
            });
        }

        public void CalculateAttributeValue(AttributeSet attributeSet, Attribute attribute)
        {
            if (!_isDirty) return;
            
            _isDirty = false;
            var sumAdditive = 0.0f;
            var sumMultiplicative = 1.0f;
            var hasOverride = false;
            var maxOverride = 0.0f;
            foreach (var data in _data)
            {
                switch (data.modifier)
                {
                    case EffectModifier.Additive:
                        sumAdditive += data.value;
                        break;
                    case EffectModifier.Multiplicative:
                        sumMultiplicative += data.value;
                        break;
                    case EffectModifier.Override:
                        hasOverride = true;
                        if (Mathf.Abs(data.value) > maxOverride)
                        {
                            maxOverride = data.value;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (hasOverride)
            {
                attributeSet.SetAttributeBaseValue(attribute, maxOverride);
            }
            else
            {
                var value = attributeSet.GetAttributeBaseValue(attribute);
                value = (value + sumAdditive) * sumMultiplicative;
                attributeSet.SetAttributeCurrentValue(attribute, value);
            }
        }
        
        public void RemoveEffect(IEffectSource effect)
        {
            _isDirty = true;
            _data.RemoveAll((data) => data.effect == effect);
        }
    }
}