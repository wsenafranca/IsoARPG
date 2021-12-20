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
        private Targetable _lastTarget;
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
            var moveDestination = StateMachineManager.GetState<MoveDestinationState>();
            var collect = StateMachineManager.GetState<CollectState>();
            var useSkill = StateMachineManager.GetState<UseSkillState>();
            
            AddTransition(wait, moveDestination, ()=> _characterMovement.isNavigating);
            AddTransition(wait, collect, ()=> _action == PlayerAction.Collect);
            AddTransition(wait, useSkill, ()=> _action == PlayerAction.UseSkill);
            
            AddTransition(moveDestination, wait, ()=> _characterMovement.hasReachDestination);
            
            AddTransition(useSkill, wait, () => !_animator.isPlayingAnimation);

            currentState = wait;

            instance = this;
        }

        private void OnEnable()
        {
            input.pointerClickGround?.AddListener(OnClickGround);
            input.pointerClickTarget?.AddListener(OnClickTarget);
        }

        private void OnDisable()
        {
            input.pointerClickGround?.RemoveListener(OnClickGround);
            input.pointerClickTarget?.RemoveListener(OnClickTarget);
        }

        public void SetDestination(Vector3 target, float acceptableDistance)
        {
            _characterMovement.SetDestination(target, acceptableDistance);
        }
        
        public void SetDestination(Transform target, float acceptableDistance)
        {
            _characterMovement.SetDestination(target, acceptableDistance);
        }

        public void Collect()
        {
            _action = PlayerAction.None;
            if (_lastTarget == null || _lastTarget is not Collectible collectible) return;
            collectible.Collect(gameObject);
        }

        public bool BeginUseSkill()
        {
            if (_animator.isPlayingAnimation || _currentSkill == null || !_currentSkill.CanUseSkill(_character))
            {
                EndUseSkill();
                return false;
            }

            if (_currentSkill.skillBase.needTarget)
            {
                if (_lastTarget == null)
                {
                    EndUseSkill();
                    return false;
                }
                
                _characterMovement.LookAt(_lastTarget.transform);
            }
            
            _hitNumber = Time.time - _lastAttack < 2.0f ? (_hitNumber + 1) % 2 : 0;

            _animator.hitHumber = _hitNumber;

            var weaponIndex = _hitNumber == 1 && _inventory.GetWeaponMelee(1) ? _hitNumber : 0;
            _animator.weaponIndex = weaponIndex;

            if (!_currentSkill.TryUseSkill(_character, out var damageIntent))
            {
                EndUseSkill();
                return false;
            }
            
            if (_currentSkill.skillBase.requirements.HasFlag(SkillRequirements.MeleeWeapon))
            {
                var weapon = _inventory.GetWeaponMelee(weaponIndex);
                if (weapon == null)
                {
                    EndUseSkill();
                    return false;
                }
                
                weapon.SetDamageIntent(damageIntent);
            }
            
            _action = PlayerAction.UseSkill;
            
            if (_animator.TriggerSkillAnimation(_currentSkill.skillBase.animatorTrigger)) return true;

            EndUseSkill();
            return false;
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
                stateMachine._action = PlayerAction.None;
                stateMachine.SetDestination(worldPoint, 0.0f);
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                if (target != null && target.enabled)
                {
                    stateMachine._lastTarget = target;
                }

                if (stateMachine._lastTarget == null || !stateMachine._lastTarget.enabled)
                {
                    stateMachine._lastTarget = null;
                    return;
                }
                
                switch (stateMachine._lastTarget.targetType)
                {
                    case TargetType.Neutral:
                        stateMachine._action = PlayerAction.None;
                        stateMachine.SetDestination(stateMachine._lastTarget.transform, 1.0f);
                        break;
                    case TargetType.Enemy:
                        if (stateMachine._skillSet.TryGetSkillFromMouseButton(button, out stateMachine._currentSkill) && stateMachine._currentSkill.CanUseSkill(stateMachine._character))
                        {
                            stateMachine._action = PlayerAction.UseSkill;
                            stateMachine.SetDestination(stateMachine._lastTarget.transform, stateMachine._currentSkill.skillBase.range);
                        }
                        else
                        {
                            stateMachine._action = PlayerAction.None;
                        }
                        break;
                    case TargetType.Talkative:
                        stateMachine._action = PlayerAction.Talk;
                        stateMachine.SetDestination(stateMachine._lastTarget.transform, 1.0f);
                        break;
                    case TargetType.Collectible:
                        stateMachine._action = PlayerAction.Collect;
                        stateMachine.SetDestination(stateMachine._lastTarget.transform, 1.0f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private class MoveDestinationState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?._characterMovement.StopMovement();
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                stateMachine._characterMovement.SetDestination(worldPoint);
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target, int button)
            {
                
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