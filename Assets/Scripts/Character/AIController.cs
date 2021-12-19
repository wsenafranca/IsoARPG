using System;
using FiniteStateMachine;
using TargetSystem;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterBase))]
    [RequireComponent(typeof(Targetable))]
    public class AIController : StateMachine
    {
        private CharacterBase _character;
        private Targetable _targetable;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _targetable = GetComponent<Targetable>();
        }

        private void OnEnable()
        {
            _character.death?.AddListener(OnDeath);
        }

        private void OnDisable()
        {
            _character.death?.RemoveListener(OnDeath);
        }
        
        private void OnDeath(CharacterBase character)
        {
            _targetable.enabled = false;
        }
    }
}