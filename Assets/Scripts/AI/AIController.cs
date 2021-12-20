using Character;
using FiniteStateMachine;
using SkillSystem;
using TargetSystem;
using UnityEngine;
using Weapon;

namespace AI
{
    [RequireComponent(typeof(CharacterBase))]
    [RequireComponent(typeof(Targetable))]
    [RequireComponent(typeof(CharacterMovement))]
    public class AIController : StateMachine
    {
        private CharacterBase _character;
        private CharacterMovement _characterMovement;
        private AIAnimator _animator;
        private Targetable _targetable;
        private AISensing _sensing;
        private SkillSet _skillSet;
        private WeaponMelee[] _weapons;

        private CharacterBase _currentTarget;
        private Vector3 _currentDestination;
        private Vector3 _initialPosition;
        private SkillInstance _currentSkill;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponent<AIAnimator>();
            _targetable = GetComponent<Targetable>();
            _sensing = GetComponentInChildren<AISensing>();
            _skillSet = GetComponent<SkillSet>();
            _weapons = GetComponentsInChildren<WeaponMelee>();

            var wait = StateMachineManager.GetState<WaitState>();
            var chase = StateMachineManager.GetState<ChaseState>();
            var moveBack = StateMachineManager.GetState<MoveBackState>();
            var useSkill = StateMachineManager.GetState<UseSkillState>();
            var dead = StateMachineManager.GetState<DeadState>();
            
            AddTransition(wait, dead, () => !_character.isAlive);
            AddTransition(wait, chase, () => isCurrentTargetValid);
            
            AddTransition(chase, dead, () => !_character.isAlive);
            AddTransition(chase, moveBack, () => !isCurrentTargetValid);
            AddTransition(chase, useSkill, () => GetFirstAvailableSkill(out _currentSkill));
            
            AddTransition(moveBack, wait, () => _characterMovement.hasReachDestination);
            AddTransition(moveBack, chase, () => isCurrentTargetValid);
            
            AddTransition(useSkill, chase, () => !_animator.isPlayingAnimation);

            currentState = wait;
        }

        private bool isCurrentTargetValid => _currentTarget != null && _currentTarget.isAlive;

        private bool GetFirstAvailableSkill(out SkillInstance skillInstance)
        {
            skillInstance = null;
            if (_skillSet == null) return false;

            foreach (var skill in _skillSet.skills)
            {
                if(!_skillSet.TryGetSkillInstance(skill, out skillInstance)) continue;

                if (skillInstance.CanUseSkillAtTarget(_character, _currentTarget))
                {
                    return true;
                }
            }
            
            return false;
        }

        private void OnEnable()
        {
            _character.death?.AddListener(OnDeath);

            if (_sensing != null)
            {
                _sensing.targetEnter.AddListener(OnTargetEnter);
                _sensing.targetExit.AddListener(OnTargetExit);
            }

            _targetable.enabled = true;
        }

        private void OnDisable()
        {
            _character.death?.RemoveListener(OnDeath);
            _currentTarget = null;
            _currentSkill = null;

            if (_sensing != null)
            {
                _sensing.targetEnter.RemoveListener(OnTargetEnter);
                _sensing.targetExit.RemoveListener(OnTargetExit);
            }
        }
        
        private void OnDeath(CharacterBase character)
        {
            _targetable.enabled = false;
            
            if (_sensing != null) _sensing.enabled = false;
        }
        
        private void OnTargetEnter(Collider other)
        {
            if (other.gameObject == gameObject) return;
            _currentTarget = other.GetComponent<CharacterBase>();
            
        }
        
        private void OnTargetExit(Collider other)
        {
            if (other.gameObject == gameObject) return;
            _currentTarget = null;
        }
        
        private bool BeginUseSkill()
        {
            if (_animator.isPlayingAnimation || _currentSkill == null || !_currentSkill.CanUseSkill(_character))
            {
                EndUseSkill();
                return false;
            }

            if (_currentSkill.skillBase.needTarget)
            {
                if (_currentTarget == null)
                {
                    EndUseSkill();
                    return false;
                }
                
                _characterMovement.LookAt(_currentTarget.transform);
            }

            if (!_currentSkill.TryUseSkill(_character, out var damageIntent))
            {
                EndUseSkill();
                return false;
            }
            
            if (_currentSkill.skillBase.requirements.HasFlag(SkillRequirements.MeleeWeapon))
            {
                var weapon = _weapons.Length > 0 ? _weapons[0] : null;
                if (weapon == null)
                {
                    EndUseSkill();
                    return false;
                }
                
                weapon.SetDamageIntent(damageIntent);
            }

            return _animator.TriggerSkillAnimation(_currentSkill.skillBase.animatorTrigger);
        }

        private void EndUseSkill()
        {
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
                if (stateMachine is not AIController aiController || aiController._animator == null) return;
                aiController._animator.chasing = false;
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
                if (stateMachine is not AIController aiController) return;

                var target = aiController._currentTarget;
                if (target == null) return;
                
                var destination = target.transform.position;
                aiController._initialPosition = aiController.transform.position;
                aiController._currentDestination = destination;
                aiController._characterMovement.SetDestination(destination, 1.0f);
                aiController._animator.chasing = true;
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                if (stateMachine is not AIController aiController) return;
                var target = aiController._currentTarget;
                if (target == null) return;

                var destination = target.transform.position;

                if (Vector3.Distance(destination, aiController._currentDestination) < 0.1f) return;
                
                aiController._currentDestination = destination;
                aiController._characterMovement.SetDestination(destination, 1.0f);
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