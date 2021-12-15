using UnityEngine;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "PoisonEffect", menuName = "AbilitySystem/Effects/PoisonEffect", order = 0)]
    public class PoisonEffect : EffectBase
    {
        public float scale = 0.02f;
        public float variance = 0.1f;

        private int _stack;

        public override void OnAddEffect(AbilitySystemComponent target)
        {
            _stack = 1;
        }

        public override void OnApplyEffect(AbilitySystemComponent target)
        {
            target.ApplyDamage(new DamageInfo
            {
                damageTarget = DamageTarget.Health,
                damage = target.attributeSet[Attribute.MaxHealth].GetCurrentValue() * scale * _stack * (1 + Random.Range(-variance, variance)),
                isBlocked = false,
                isCritical = false,
                ignoreMagicShield = true
            });
        }

        public override void OnRemoveEffect(AbilitySystemComponent target)
        {
            
        }

        public override void OnAddStack(AbilitySystemComponent target, int stack)
        {
            _stack++;
        }
    }
}