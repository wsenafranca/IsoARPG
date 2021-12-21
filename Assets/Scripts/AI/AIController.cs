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
        private AIAggro _aggro;

        private CharacterBase _currentTarget;
        private Vector3 _initialPosition;
        private SkillInstance _currentSkill;
        private float _waitTime;

        private bool isCurrentTargetValid => _currentTarget != null && _currentTarget.isAlive;

        private bool isCurrentSkillValid => _currentSkill != null;
        
        private bool isInSkillRange => isCurrentSkillValid && isCurrentTargetValid && _characterMovement.Distance(_currentTarget.characterMovement) < _currentSkill.skillBase.range;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponent<AIAnimator>();
            _targetable = GetComponent<Targetable>();
            _sensing = GetComponentInChildren<AISensing>();
            _skillSet = GetComponent<SkillSet>();
            _weapons = GetComponentsInChildren<WeaponMelee>();
            _aggro = GetComponent<AIAggro>();

            var wait = StateMachineManager.GetState<WaitState>();
            var alert = StateMachineManager.GetState<AlertState>();
            var chase = StateMachineManager.GetState<ChaseState>();
            var moveBack = StateMachineManager.GetState<MoveBackState>();
            var useSkill = StateMachineManager.GetState<UseSkillState>();
            var afterUseSkill = StateMachineManager.GetState<AfterUseSkillState>();
            var dead = StateMachineManager.GetState<DeadState>();
            
            AddTransition(wait, dead, () => !_character.isAlive);
            AddTransition(wait, alert, () => isCurrentTargetValid);
            
            AddTransition(alert, dead, () => !_character.isAlive);
            AddTransition(alert, wait, () => !isCurrentTargetValid);
            AddTransition(alert, chase, () => isCurrentTargetValid && TryGetFirstAvailableSkill(out _currentSkill));
            
            AddTransition(chase, dead, () => !_character.isAlive);
            AddTransition(chase, moveBack, () => !isCurrentTargetValid);
            AddTransition(chase, alert, () => !isCurrentSkillValid);
            AddTransition(chase, useSkill, () => isInSkillRange);

            AddTransition(moveBack, dead, () => !_character.isAlive);
            AddTransition(moveBack, wait, () => _characterMovement.hasReachDestination);
            AddTransition(moveBack, alert, () => isCurrentTargetValid);
            
            AddTransition(useSkill, afterUseSkill, () => !_animator.isPlayingAnimation);
            
            AddTransition(afterUseSkill, dead, () => !_character.isAlive);
            AddTransition(afterUseSkill, alert, () => currentStateElapsedTime > _waitTime);
        }
        
        private void OnEnable()
        {
            _character.dead?.AddListener(OnDead);

            if (_sensing)
            {
                _sensing.targetUpdate.AddListener(OnTargetUpdate);
                _sensing.targetExit.AddListener(OnTargetExit);
                _sensing.enabled = true;
            }

            if (_aggro)
            {
                _aggro.aggroChanged.AddListener(OnAggroChanged);
                _aggro.enabled = true;
            }

            _targetable.enabled = true;
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

            if (_sensing)
            {
                _sensing.targetUpdate.RemoveListener(OnTargetUpdate);
                _sensing.targetExit.RemoveListener(OnTargetExit);
            }

            if (_aggro)
            {
                _aggro.aggroChanged.RemoveListener(OnAggroChanged);
            }
        }
        
        private bool TryGetFirstAvailableSkill(out SkillInstance skillInstance)
        {
            skillInstance = null;
            if (_skillSet == null) return false;
            
            foreach (var skill in _skillSet.skills)
            {
                if(!_skillSet.TryGetSkillInstance(skill, out skillInstance)) continue;

                if (skillInstance.CanUseSkill(_character))
                {
                    return true;
                }
            }
            
            return false;
        }

        private void OnDead(CharacterBase character)
        {
            _targetable.enabled = false;
            
            if (_sensing != null) _sensing.enabled = false;
            if (_aggro != null) _aggro.enabled = false;
        }

        private void OnTargetUpdate(CharacterBase target)
        {
            _currentTarget = target;
        }

        private void OnTargetExit(CharacterBase target)
        {
            if(_aggro) _aggro.RemoveTarget(target);
        }
        
        private void OnAggroChanged(CharacterBase target)
        {
            if (_sensing != null)
            {
                if(_sensing.IsSensing(target)) _sensing.currentTarget = target;
            }
            else
            {
                _currentTarget = target;
            }
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
                if (_currentTarget == null)
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

        private class AfterUseSkillState : IState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not AIController aiController) return;
                aiController._waitTime = Random.Range(0.5f, 2.0f);
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
            }

            public void OnStateExit(StateMachine stateMachine)
            {
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