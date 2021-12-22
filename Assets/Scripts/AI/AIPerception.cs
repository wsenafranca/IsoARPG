using System;
using System.Collections.Generic;
using System.Linq;
using AI.Stimulus;
using Character;
using Damage;
using SkillSystem;
using UnityEngine;

namespace AI
{
    public class AIPerception : MonoBehaviour
    {
        public List<DamageStimulusData> damageStimulusData;
        public List<SightStimulusData> sightStimulusData;
        
        private readonly List<BaseStimulus> _opponentStimulus = new();
        private readonly List<BaseStimulus> _partnerStimulus = new();
        
        private CharacterBase _character;

        public bool TryGetOpponentTarget(out CharacterBase target) => TryGetTarget(_opponentStimulus, out target);
        public bool TryGetPartnerTarget(out CharacterBase target) => TryGetTarget(_partnerStimulus, out target);

        public bool TryGetTargetForSkill(SkillInstance skill, out CharacterBase target)
        {
            target = null;

            return skill != null && skill.skillBase.damageTargetType switch
            {
                DamageTargetType.Opponent => TryGetOpponentTarget(out target),
                DamageTargetType.Partner => TryGetPartnerTarget(out target),
                DamageTargetType.Self => (target = _character),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool hasOpponent => _opponentStimulus.Sum(stimulus => stimulus.countTargets) > 0;
        
        public bool hasPartner => _partnerStimulus.Sum(stimulus => stimulus.countTargets) > 0;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();

            foreach (var data in damageStimulusData)
            {
                _opponentStimulus.Add(new DamageStimulus(_character, data.priority));
            }
            
            foreach (var data in sightStimulusData)
            {
                switch (data.filterTarget)
                {
                    case DamageTargetType.Opponent:
                        _opponentStimulus.Add(new SightStimulus(_character, data.priority, data.sensing, data.filterTarget));
                        break;
                    case DamageTargetType.Partner:
                        _partnerStimulus.Add(new SightStimulus(_character, data.priority, data.sensing, data.filterTarget));
                        break;
                    case DamageTargetType.Self:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnEnable()
        {
            foreach (var stimulus in _opponentStimulus)
            {
                stimulus.OnEnable();
            }
            
            foreach (var stimulus in _partnerStimulus)
            {
                stimulus.OnEnable();
            }
        }
        
        private void OnDisable()
        {
            foreach (var stimulus in _opponentStimulus)
            {
                stimulus.OnDisable();
            }
            _opponentStimulus.Clear();
            
            foreach (var stimulus in _partnerStimulus)
            {
                stimulus.OnDisable();
            }
            _partnerStimulus.Clear();
        }

        private static bool TryGetTarget(IReadOnlyCollection<BaseStimulus> list, out CharacterBase target)
        {
            if (list == null)
            {
                target = null;
                return false;
            }
            
            foreach (var stimulus in list.OrderBy(stimulus => stimulus.priority))
            {
                target = stimulus.currentTarget;
                if (target != null) return true;
            }

            target = null;
            return false;
        }
    }
}