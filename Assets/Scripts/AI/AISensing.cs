using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class AISensing : MonoBehaviour
    {
        public UnityEvent<CharacterBase> targetUpdate;
        public UnityEvent<CharacterBase> targetEnter;
        public UnityEvent<CharacterBase> targetExit;

        private readonly List<CharacterBase> _targets = new();

        private CharacterBase _currentTarget;

        public CharacterBase currentTarget
        {
            get => _currentTarget;
            set
            {
                if (value == _currentTarget) return;

                if (value != null && !_targets.Contains(value))
                {
                    AddTarget(value);
                }

                _currentTarget = value;
                targetUpdate?.Invoke(_currentTarget);
            }
        }

        public bool IsSensing(CharacterBase target)
        {
            return target != null && _targets.Contains(target);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            var target = other.GetComponent<CharacterBase>();
            AddTarget(target);
            
            if (currentTarget == null) currentTarget = target;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            var target = other.GetComponent<CharacterBase>();
            RemoveTarget(target);
            
            if (currentTarget == target) currentTarget = _targets.FirstOrDefault();
        }

        private void AddTarget(CharacterBase character)
        {
            if (character == null || _targets.Contains(character)) return;
            
            _targets.Add(character);
            character.dead.AddListener(OnTargetDead);
            targetEnter?.Invoke(character);
        }

        private void RemoveTarget(CharacterBase character)
        {
            if (character == null) return;

            _targets.Remove(character);
            character.dead.RemoveListener(OnTargetDead);
            targetExit?.Invoke(character);
        }

        private void OnTargetDead(CharacterBase character)
        {
            RemoveTarget(character);
            
            if (currentTarget == null) currentTarget = _targets.FirstOrDefault();
        }
    }
}