using System;
using System.Collections.Generic;
using Character;
using Damage;

namespace AI.Stimulus
{
    [Serializable]
    public struct DamageStimulusData
    {
        public int priority;
    }
    
    public class DamageStimulus : BaseStimulus
    {
        private readonly Dictionary<CharacterBase, int> _targets = new();

        public DamageStimulus(CharacterBase owner, int priority) : base(owner, priority) {}

        public override int countTargets => _targets.Count;

        public override CharacterBase currentTarget
        {
            get
            {
                var maxDamage = 0;
                CharacterBase selectedTarget = null;

                foreach (var (target, damage) in _targets)
                {
                    if(damage < maxDamage) continue;

                    maxDamage = damage;
                    selectedTarget = target;
                }
                
                return selectedTarget;
            }
        }

        public override void OnEnable()
        {
            
        }

        public override void OnDisable()
        {
            foreach (var (target, _) in _targets)
            {
                target.dead.RemoveListener(OnTargetDead);
            }
            _targets.Clear();
        }
        
        private void RemoveTarget(CharacterBase target)
        {
            if (target == null || !_targets.ContainsKey(target)) return;
            
            _targets.Remove(target);
            target.dead.RemoveListener(OnTargetDead);
            NotifyCharacterExit(target);
        }

        private void OnDamageReceived(CharacterBase _, DamageHit damage)
        {
            var target = damage.instigator;
            if (target == null || damage.damageType != DamageType.Health || damage.value <= 0) return;
            
            if (_targets.TryGetValue(target, out var value))
            {
                _targets[target] = value + damage.value;
                return;
            }

            _targets[target] = damage.value;
            target.dead.AddListener(OnTargetDead);
            NotifyCharacterEnter(target);
        }

        private void OnTargetDead(CharacterBase target)
        {
            RemoveTarget(target);
        }
    }
}