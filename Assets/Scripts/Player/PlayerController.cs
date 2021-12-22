﻿using AI;
using Character;
using DialogSystem;
using FiniteStateMachine;
using Item;
using SkillSystem;
using UnityEngine;

namespace Player
{
    public class PlayerController : StateMachine
    {
        public static PlayerController instance { get; private set; }

        public string displayName => _character.displayName;
        
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
        private TargetBase _currentTarget;
        private CharacterBase _currentTargetCharacter;
        private Vector3 _currentDestination;
        private float _currentAcceptableDistance;
        private SkillInstance _currentSkill;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponent<PlayerAnimator>();
            _inventory = GetComponent<PlayerInventoryController>();
            _skillSet = GetComponent<SkillSet>();
            input = GetComponent<InputController>();

            var wait = GetState<WaitState>();
            var move = GetState<MoveState>();
            var moveToRangeSkill = GetState<MoveToRangeSkillState>();
            var collect = GetState<CollectState>();
            var useSkill = GetState<UseSkillState>();
            var talk = GetState<TalkState>();
            
            AddTransition(wait, move, ()=> _action == PlayerAction.Move);
            AddTransition(wait, move, ()=> _action == PlayerAction.Collect);
            AddTransition(wait, move, ()=> _action == PlayerAction.Talk);
            AddTransition(wait, moveToRangeSkill, ()=> _action == PlayerAction.UseSkill);
            
            AddTransition(move, collect, ()=> _characterMovement.hasReachDestination && _action == PlayerAction.Collect);
            AddTransition(move, talk, ()=> _characterMovement.hasReachDestination && _action == PlayerAction.Talk);
            AddTransition(move, wait, ()=> _characterMovement.hasReachDestination);
            AddTransition(move, wait, ()=> !_characterMovement.isNavigating);
            
            AddTransition(moveToRangeSkill, wait, () => _action != PlayerAction.UseSkill);
            AddTransition(moveToRangeSkill, useSkill, () => isInSkillRange);
            
            AddTransition(useSkill, wait, () => !_animator.isPlayingAnimation);
            
            AddTransition(talk, wait, () => !isCurrentTargetValid);

            instance = this;
        }
        
        private bool isCurrentTargetValid => _currentTarget != null && _currentTarget.isTargetValid;
        
        private bool isCurrentSkillValid => _currentSkill != null;
        
        private bool isInSkillRange => isCurrentSkillValid && isCurrentTargetValid && _characterMovement.Distance(_currentTargetCharacter.characterMovement) < _currentSkill.skillBase.range;

        private void OnEnable()
        {
            input.pointerClickGround?.AddListener(OnClickGround);
            input.pointerClickTarget?.AddListener(OnClickTarget);
        }

        private void Start()
        {
            currentState = GetState<WaitState>();
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

        public void BeginTalk()
        {
        }

        public void EndTalk()
        {
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
                if (!_currentSkill.IsTargetValid(_character, _currentTargetCharacter))
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
        
        private void OnClickTarget(TargetBase target, int button)
        {
            GetCurrentState<IPlayerState>()?.OnClickTarget(this, target, button);
        }

        private void MoveToDestination(Vector3 worldPoint, float acceptableDistance)
        {
            _currentDestination = worldPoint;
            _currentAcceptableDistance = acceptableDistance;
            _action = PlayerAction.Move;
            _currentTarget = null;
            _currentTargetCharacter = null;
        }

        private void MoveToTarget(TargetBase target, int button)
        {
            if (target != null && target.isTargetValid)
            {
                _currentTarget = target;
            }

            if (_currentTarget == null || !_currentTarget.isTargetValid)
            {
                _currentTarget = null;
                _currentTargetCharacter = null;
                return;
            }

            _action = PlayerAction.None;
            switch (_currentTarget)
            {
                case Collectible:
                    _action = PlayerAction.Collect;
                    _currentDestination = target.transform.position;
                    break;
                case AITarget characterTarget:
                    if (_skillSet.TryGetSkillInstance(input.skillSlot[button], out _currentSkill)
                        && _currentSkill.CanUseSkill(_character)
                        && _currentSkill.IsTargetValid(_character, characterTarget.character))
                    {
                        _action = PlayerAction.UseSkill;
                        _currentTargetCharacter = characterTarget.character;
                    }
                    else
                    {
                        _currentTarget = null;
                        _currentTargetCharacter = null;
                    }
                    break;
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
            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button);
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
                stateMachine.MoveToDestination(worldPoint, 0.0f);
            }

            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button)
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
                    stateMachine.MoveToDestination(worldPoint, 0.0f);
                }
            }

            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button)
            {
                if(target is AITarget) stateMachine.MoveToTarget(target, button);
            }
        }

        private class MoveState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                if (stateMachine is not PlayerController playerController) return;
                playerController._characterMovement.SetDestination(playerController._currentDestination, playerController._currentAcceptableDistance);
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
                stateMachine.MoveToDestination(worldPoint, 0.0f);
                stateMachine._characterMovement.SetDestination(worldPoint);
            }

            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button)
            {
                if(target is AITarget) stateMachine.MoveToTarget(target, button);
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

            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button)
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

            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button)
            {
                
            }
        }

        private class TalkState : IPlayerState
        {
            public void OnStateEnter(StateMachine stateMachine)
            {
                (stateMachine as PlayerController)?.BeginTalk();
            }

            public void OnStateUpdate(StateMachine stateMachine, float elapsedTime)
            {
                
            }

            public void OnStateExit(StateMachine stateMachine)
            {
                
            }

            public void OnClickGround(PlayerController stateMachine, Vector3 worldPoint)
            {
                stateMachine.EndTalk();
            }

            public void OnClickTarget(PlayerController stateMachine, TargetBase target, int button)
            {
                stateMachine.EndTalk();
            }
        }
    }
}