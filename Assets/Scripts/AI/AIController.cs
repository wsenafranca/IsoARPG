﻿using Character;
using FiniteStateMachine;
using SkillSystem;
using UnityEngine;
using Weapon;

namespace AI
{
    [RequireComponent(typeof(CharacterBase))]
    [RequireComponent(typeof(CharacterMovement))]
    public class AIController : StateMachine
    {
        private CharacterBase _character;
        private CharacterMovement _characterMovement;
        private AIAnimator _animator;
        private AIPerception _perception;
        private SkillSet _skillSet;
        private WeaponMelee[] _weapons;

        private CharacterBase _currentTarget;
        private Vector3 _initialPosition;
        private SkillInstance _currentSkill;
        private float _waitTime;

        private bool isCurrentTargetValid => _currentTarget != null && _currentTarget.isAlive;

        private bool isCurrentSkillValid => _currentSkill != null;
        
        private bool isInSkillRange => isCurrentSkillValid && isCurrentTargetValid && _characterMovement.Distance(_currentTarget.characterMovement) < _currentSkill.skillBase.range;

        private bool isSensingOpponent => _perception != null && _perception.hasOpponent;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponent<AIAnimator>();
            _perception = GetComponentInChildren<AIPerception>();
            _skillSet = GetComponent<SkillSet>();
            _weapons = GetComponentsInChildren<WeaponMelee>();

            var wait = GetState<WaitState>();
            var alert = GetState<AlertState>();
            var chase = GetState<ChaseState>();
            var moveBack = GetState<MoveBackState>();
            var useSkill = GetState<UseSkillState>();
            var dead = GetState<DeadState>();
            
            AddTransition(wait, dead, () => !_character.isAlive);
            AddTransition(wait, alert, () => isSensingOpponent);
            
            AddTransition(alert, dead, () => !_character.isAlive);
            AddTransition(alert, wait, () => !isSensingOpponent);
            AddTransition(alert, chase, () => currentStateElapsedTime > _waitTime && FindAvailableSkill());
            
            AddTransition(chase, dead, () => !_character.isAlive);
            AddTransition(chase, moveBack, () => !isCurrentTargetValid);
            AddTransition(chase, alert, () => !isCurrentSkillValid);
            AddTransition(chase, useSkill, () => isInSkillRange);

            AddTransition(moveBack, dead, () => !_character.isAlive);
            AddTransition(moveBack, wait, () => _characterMovement.hasReachDestination);
            AddTransition(moveBack, alert, () => isCurrentTargetValid);
            
            AddTransition(useSkill, alert, () => !_animator.isPlayingAnimation);
        }
        
        private void OnEnable()
        {
            _character.dead?.AddListener(OnDead);
            if (_perception != null) _perception.enabled = true;
        }

        private void Start()
        {
            _initialPosition = transform.position;
            currentState = StateMachineManager.GetState<WaitState>();
        }

        private void OnDisable()
        {
            _character.dead?.RemoveListener(OnDead);
            _currentTarget = null;
            _currentSkill = null;
        }
        
        private bool FindAvailableSkill()
        {
            if (_skillSet == null) return false;
            
            foreach (var skill in _skillSet.skills)
            {
                if(!_skillSet.TryGetSkillInstance(skill, out _currentSkill)) continue;

                if (!_currentSkill.CanUseSkill(_character)) continue;

                if(_perception.TryGetTargetForSkill(_currentSkill, out _currentTarget))
                {
                    return true;
                }
            }

            _currentSkill = null;
            return false;
        }

        private void OnDead(CharacterBase character)
        {
            OnDisable();
        }
        
        private void BeginUseSkill()
        {
            if (_animator.isPlayingAnimation || _currentSkill == null || !_currentSkill.CanUseSkill(_character))
            {
                EndUseSkill();
                return;
            }

            if (_currentSkill.skillBase.needTarget)
            {
                if (!_currentSkill.IsTargetValid(_character, _currentTarget))
                {
                    EndUseSkill();
                    return;
                }
                
                _characterMovement.LookAt(_currentTarget.transform);
            }

            if (!_currentSkill.TryUseSkill(_character, out var damageIntent))
            {
                EndUseSkill();
                return;
            }
            
            if (_currentSkill.skillBase.requirements.HasFlag(SkillRequirements.MeleeWeapon))
            {
                var weapon = _weapons.Length > 0 ? _weapons[0] : null;
                if (weapon == null)
                {
                    EndUseSkill();
                    return;
                }
                
                weapon.SetDamageIntent(damageIntent);
            }

            _animator.TriggerSkillAnimation(_currentSkill.skillBase.animatorTrigger);
        }
        
        private void EndUseSkill()
        {
            _currentSkill = null;
            _currentTarget = null;
        }
        
        private void OnWeaponBeginAttack(int index)
        {
            if (index >= _weapons.Length || _weapons[index] == null) return;
            _weapons[index].BeginAttack(gameObject);
        }

        private void OnWeaponEndAttack(int index)
        {
            if (index >= _weapons.Length || _weapons[index] == null) return;
            _weapons[index].EndAttack();
        }
        
        private class WaitState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                aiController._animator.chasing = false;
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                
            }
        }

        private class AlertState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                aiController._animator.chasing = true;

                if (aiController._currentTarget != null) aiController._characterMovement.LookAt(aiController._currentTarget.transform);
                
                aiController._waitTime = Random.Range(0.0f, 1.0f);
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
            }

            public void OnStateExit(StateMachine stateMachine)
            {
            }
        }
        
        private class ChaseState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                if (stateMachine is not AIController aiController) return;
                var target = aiController._currentTarget;
                if (target == null || aiController._currentSkill == null) return;
                
                aiController._characterMovement.SetDestination(target.transform.position, aiController._currentSkill.skillBase.range);
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                
                aiController._characterMovement.StopMovement();
            }
        }

        private class UseSkillState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                
                aiController.BeginUseSkill();
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                
                aiController.EndUseSkill();
            }
        }
        
        private class MoveBackState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;

                aiController._characterMovement.SetDestination(aiController._initialPosition);
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                
                aiController._characterMovement.StopMovement();
            }
        }

        private class DeadState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
            }

            public void OnStateExit(StateMachine stateMachine)
            {
            }
        }
    }
}