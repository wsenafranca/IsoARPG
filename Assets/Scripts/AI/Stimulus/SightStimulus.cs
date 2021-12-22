using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Damage;
using UnityEngine;

namespace AI.Stimulus
{
    [Serializable]
    public struct SightStimulusData
    {
        public int priority;
        public AISensing sensing;
        public DamageTargetType filterTarget;
    }
    
    public class SightStimulus : BaseStimulus
    {
        private readonly AISensing _sensing;
        private readonly DamageTargetType _filterTarget;
        private readonly HashSet<CharacterBase> _targets = new();

        public SightStimulus(CharacterBase owner, int priority, AISensing sensing, DamageTargetType filterTarget) : base(owner, priority)
        {
            _sensing = sensing;
            _filterTarget = filterTarget;
        }

        public override int countTargets => _targets.Count(IsTargetValid);

        public override CharacterBase currentTarget
        {
            get
            {
                var minDistance = 999999.0f;
                CharacterBase minTarget = null;
                foreach (var target in _targets.Where(IsTargetValid))
                {
                    var distance = Vector3.Distance(owner.transform.position, target.transform.position);
                    if (distance > minDistance) continue;
                    
                    minDistance = distance;
                    minTarget = target;
                }

                return minTarget;
            }
        }

        public override void OnEnable()
        {
            _sensing.targetEnter.AddListener(OnTargetEnter);
            _sensing.targetExit.AddListener(OnTargetExit);
        }

        public override void OnDisable()
        {
            foreach (var target in _targets)
            {
                target.dead.RemoveListener(OnTargetDead);
            }
            _targets.Clear();
            
            _sensing.targetEnter.RemoveListener(OnTargetEnter);
            _sensing.targetExit.RemoveListener(OnTargetExit);
        }

        private bool IsTargetValid(CharacterBase target)
        {
            return _filterTarget switch
            {
                DamageTargetType.Opponent => owner.IsOpponent(target),
                DamageTargetType.Partner => owner.IsPartner(target),
                DamageTargetType.Self => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private void RemoveTarget(CharacterBase target)
        {
            if (target == null || !_targets.Contains(target)) return;
            
            target.dead.RemoveListener(OnTargetDead);
            _targets.Remove(target);
        }
        
        private void OnTargetEnter(Collider other)
        {
            var target = other.GetComponent<CharacterBase>();
            
            if (target == null) return;

            if (_targets.Contains(target)) return;
            
            _targets.Add(target);
            target.dead.AddListener(OnTargetDead);
        }

        private void OnTargetExit(Collider other)
        {
            RemoveTarget(other.GetComponent<CharacterBase>());
        }
        
        private void OnTargetDead(CharacterBase target)
        {
            RemoveTarget(target);
        }
    }
}