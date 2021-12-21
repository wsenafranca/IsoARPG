using System;
using Character;
using FiniteStateMachine;
using Item;
using SkillSystem;
using TargetSystem;
using UnityEngine;

namespace Player
{
    public class PlayerController : StateMachine
    {
        public static PlayerController instance { get; private set; }
        
        private CharacterBase _character;
        private CharacterMovement _characterMovement;
        private PlayerAnimator _animator;
        private PlayerInventoryController _inventory;
        private SkillSet _skillSet;
        public InputController input { get; private set; }
        
        private PlayerAction _action;
        private int _hitNumber;
        private float _lastAttack;
        private bool _isPressing;
        private Targetable _currentTarget;
        private CharacterBase _currentTargetCharacter;
        private Vector3 _currentDestination;
        private SkillInstance _currentSkill;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponent<PlayerAnimator>();
            _inventory = GetComponent<PlayerInventoryController>();
            _skillSet = GetComponent<SkillSet>();
            input = GetComponent<InputController>();

            var wait = StateMachineManager.GetState<WaitState>();
            var move = StateMachineManager.GetState<MoveState>();
            var moveToRangeSkill = StateMachineManager.GetState<MoveToRangeSkillState>();
            var collect = StateMachineManager.GetState<CollectState>();
            var useSkill = StateMachineManager.GetState<UseSkillState>();
            
            AddTransition(wait, move, ()=> _action == PlayerAction.Move);
            AddTransition(wait, move, ()=> _action == PlayerAction.Collect);
            AddTransition(wait, moveToRangeSkill, ()=> _action == PlayerAction.UseSkill);
            
            AddTransition(move, collect, ()=> _characterMovement.hasReachDestination && _action == PlayerAction.Collect);
            AddTransition(move, wait, ()=> _characterMovement.hasReachDestination);
            AddTransition(move, wait, ()=> !_characterMovement.isNavigating);
            
            AddTransition(moveToRangeSkill, wait, () => _action != PlayerAction.UseSkill);
            AddTransition(moveToRangeSkill, useSkill, () => isInSkillRange);
            
            AddTransition(useSkill, wait, () => !_animator.isPlayingAnimation);

            instance = this;
        }
        
        private bool isCurrentTargetValid => _currentTarget != null && _currentTarget.enabled;
        
        private bool isCurrentSkillValid => _currentSkill != null;
        
        private bool isInSkillRange => isCurrentSkillValid && isCurrentTargetValid && _characterMovement.Distance(_currentTargetCharacter.characterMovement) < _currentSkill.skillBase.range;

        private void OnEnable()
        {
            input.pointerClickGround?.AddListener(OnClickGround);
            input.pointerClickTarget?.AddListener(OnClickTarget);
        }

        private void Start()
        {
            currentState = StateMachineManager.GetState<WaitState>();
        }

        private void OnDisable()
        {
            input.pointerClickGround?.RemoveListener(OnClickGround);
            input.pointerClickTarget?.RemoveListener(OnClickTarget);
        }

        public void Collect()
        {
            _action = PlayerAction.None;
            if (_currentTarget == null || _currentTarget is not Collectible collectible) return;
            collectible.Collect(gameObject);
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
            
            _hitNumber = Time.time - _lastAttack < 2.0f ? (_hitNumber + 1) % 2 : 0;

            _animator.hitHumber = _hitNumber;

            var weaponIndex = _hitNumber == 1 && _inventory.GetWeaponMelee(1) ? _hitNumber : 0;
            _animator.weaponIndex = weaponIndex;

            if (!_currentSkill.TryUseSkill(_character, out var damageIntent))
            {
                EndUseSkill();
                return;
            }
            
            if (_currentSkill.skillBase.requirements.HasFlag(SkillRequirements.MeleeWeapon))
            {
                var weapon = _inventory.GetWeaponMelee(weaponIndex);
                if (weapon == null)
                {
                    EndUseSkill();
                    return;
                }
                
                weapon.SetDamageIntent(damageIntent);
            }
            
            _action = PlayerAction.UseSkill;
            
            if (_animator.TriggerSkillAnimation(_currentSkill.skillBase.animatorTrigger)) return;

            EndUseSkill();
        }

        private void EndUseSkill()
        {
            _lastAttack = Time.time;
            _action = PlayerAction.None;
        }

        private void OnClickGround(Vector3 worldPoint)
        {
            GetCurrentState<IPlayerState>()?.OnClickGround(this, worldPoint);
        }
        
        private void OnClickTarget(Targetable target, int button)
        {
            GetCurrentState<IPlayerState>()?.OnClickTarget(this, target, button);
        }

        private void MoveToDestination(Vector3 worldPoint)
        {
            _currentDestination = worldPoint;
            _action = PlayerAction.Move;
            _currentTarget = null;
            _currentTargetCharacter = null;
        }

        private void MoveToTarget(Targetable target, int button)
        {
            if (target != null && target.enabled)
            {
                _currentTarget = target;
                _currentTargetCharacter = target.GetComponent<CharacterBase>();
            }

            if (_currentTarget == null || !_currentTarget.enabled)
            {
                _currentTarget = null;
                _currentTargetCharacter = null;
                return;
            }
                
            switch (_currentTarget.targetType)
            {
                case TargetType.Neutral:
                    _action = PlayerAction.None;
                    break;
                case TargetType.Enemy:
                    if (_skillSet.TryGetSkillInstance(input.skillSlot[button], out _currentSkill) && _currentSkill.CanUseSkill(_character))
                    {
                        _action = PlayerAction.UseSkill;
                    }
                    else
                    {
                        _action = PlayerAction.None;
                    }
                    break;
                case TargetType.Talkative:
                    _action = PlayerAction.Talk;
                    break;
                case TargetType.Collectible:
                    _action = PlayerAction.Collect;
                    _currentDestination = target.transform.position;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void OnWeaponBeginAttack(int index)
        {
            if(_inventory) _inventory.GetWeaponMelee(index)?.BeginAttack(gameObject);
        }

        private void OnWeaponEndAttack(int index)
        {
            if(_inventory) _inventory.GetWeaponMelee(index)?.EndAttack();
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(16, 16, 300, 30), currentState.ToString());
        }

        private enum PlayerAction
        {
            None,
            Move,
            UseSkill,
            Talk,
            Collect
        }

        private interface IPlayerState : IState
        {
            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint);
            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button);
        }

        private class WaitState : IPlayerState
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

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                stateMachine.MoveToDestination(worldPoint);
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                stateMachine.MoveToTarget(target, button);
            }
        }

        private class MoveToRangeSkillState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                if (stateMachine is not PlayerController playerController) return;
                var target = playerController._currentTargetCharacter;
                if (target == null || playerController._currentSkill == null) return;

                playerController._characterMovement.SetDestination(target.transform.position, playerController._currentSkill.skillBase.range);
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                if (stateMachine is not PlayerController playerController) return;
                playerController._characterMovement.StopMovement();
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                if (!stateMachine.isCurrentTargetValid ||
                    Vector3.Distance(worldPoint, stateMachine._currentTarget.transform.position) > 2)
                {
                    stateMachine.MoveToDestination(worldPoint);
                }
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                if(target.targetType == TargetType.Enemy) stateMachine.MoveToTarget(target, button);
            }
        }

        private class MoveState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not PlayerController playerController) return;
                playerController._characterMovement.SetDestination(playerController._currentDestination);
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                if (stateMachine is not PlayerController playerController) return;
                playerController._characterMovement.StopMovement();
                playerController._action = PlayerAction.None;
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                stateMachine.MoveToDestination(worldPoint);
                stateMachine._characterMovement.SetDestination(worldPoint);
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                if(target.targetType == TargetType.Enemy) stateMachine.MoveToTarget(target, button);
            }
        }

        private class UseSkillState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.BeginUseSkill();
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.EndUseSkill();
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                
            }
        }

        private class CollectState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.Collect();
                stateMachine.currentState = StateMachineManager.GetState<WaitState>();
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.Collect();
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                
            }
        }
    }
}