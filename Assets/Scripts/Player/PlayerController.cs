using System;
using Character;
using Damage;
using FiniteStateMachine;
using Item;
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
        public InputController input { get; private set; }
        
        private PlayerAction _action;
        private int _hitNumber;
        private float _lastAttack;
        private bool _isPressing;
        private Targetable _lastTarget;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponent<PlayerAnimator>();
            _inventory = GetComponent<PlayerInventoryController>();
            input = GetComponent<InputController>();

            var wait = StateMachineManager.GetState<WaitState>();
            var moveDestination = StateMachineManager.GetState<MoveDestinationState>();
            var collect = StateMachineManager.GetState<CollectState>();
            var normalAttack = StateMachineManager.GetState<NormalAttackState>();
            
            AddTransition(wait, moveDestination, ()=> _characterMovement.isNavigating);
            AddTransition(wait, collect, ()=> _action == PlayerAction.Collect);
            AddTransition(wait, normalAttack, ()=> _action == PlayerAction.NormalAttack);
            
            AddTransition(moveDestination, wait, ()=> _characterMovement.hasReachDestination);
            
            AddTransition(normalAttack, wait, () => !_animator.isPlayingAnimation);

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

        public void BeginNormalAttack()
        {
            if (_lastTarget == null) return;
            
            _characterMovement.LookAt(_lastTarget.transform);
            
            _hitNumber = Time.time - _lastAttack < 2.0f ? (_hitNumber + 1) % 2 : 0;

            _animator.hitHumber = _hitNumber;

            var weaponIndex = _hitNumber == 1 && _inventory.GetWeaponMelee(1) ? _hitNumber : 0;
            _animator.weaponIndex = weaponIndex;
            
            var weapon = _inventory.GetWeaponMelee(weaponIndex);
            if (weapon == null)
            {
                EndNormalAttack();
                return;
            }

            var damageIntent = new DamageIntent
            {
                source =  _character,
                damageType = DamageType.Health,
            };
            weapon.SetDamageIntent(damageIntent);
            _action = PlayerAction.NormalAttack;
            _animator.PlayNormalAttackAnimation();
        }

        public void EndNormalAttack()
        {
            _lastAttack = Time.time;
            _action = PlayerAction.None;
        }

        private void OnClickGround(Vector3 worldPoint)
        {
            GetCurrentState<IPlayerState>()?.OnClickGround(this, worldPoint);
        }

        private void OnClickTarget(Targetable target)
        {
            GetCurrentState<IPlayerState>()?.OnClickTarget(this, target);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(16, 16, 300, 30), currentState.ToString());
        }

        private enum PlayerAction
        {
            None,
            NormalAttack,
            Talk,
            Collect
        }

        private interface IPlayerState : IState
        {
            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint);
            public void OnClickTarget(PlayerController stateMachine, Targetable target);
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

            public void OnClickTarget(PlayerController stateMachine, Targetable target)
            {
                if (target != null && target.isValid)
                {
                    stateMachine._lastTarget = target;
                }

                if (stateMachine._lastTarget == null || !stateMachine._lastTarget.isValid)
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
                        stateMachine._action = PlayerAction.NormalAttack;
                        stateMachine.SetDestination(stateMachine._lastTarget.transform, 1.0f);
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

            public void OnClickTarget(PlayerController stateMachine, Targetable target)
            {
                
            }
        }

        private class NormalAttackState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.BeginNormalAttack();
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.EndNormalAttack();
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                
            }

            public void OnClickTarget(PlayerController stateMachine, Targetable target)
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

            public void OnClickTarget(PlayerController stateMachine, Targetable target)
            {
                
            }
        }
    }
}