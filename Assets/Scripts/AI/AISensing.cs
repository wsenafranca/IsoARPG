using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class AISensing : MonoBehaviour
    {
        public UnityEvent<CharacterBase> targetEnter;
        public UnityEvent<CharacterBase> targetExit;

        private readonly List<CharacterBase> _targets = new();

        public bool IsSensing(CharacterBase target)
        {
            return target != null && _targets.Contains(target);
        }

        public bool isSensingAnyCharacter => _targets.Count > 0;

        public bool FindTarget(Func<CharacterBase, bool> pred, out CharacterBase target)
        {
            target = _targets.FirstOrDefault(pred);
            return target != null;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            AddTarget(other.GetComponent<CharacterBase>());
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            RemoveTarget(other.GetComponent<CharacterBase>());
        }

        public void AddTarget(CharacterBase character)
        {
            if (character == null || _targets.Contains(character)) return;
            
            _targets.Add(character);
            character.dead.AddListener(OnTargetDead);
            targetEnter?.Invoke(character);
        }

        public void RemoveTarget(CharacterBase character)
        {
            if (character == null) return;

            _targets.Remove(character);
            character.dead.RemoveListener(OnTargetDead);
            targetExit?.Invoke(character);
        }

        private void OnTargetDead(CharacterBase character)
        {
            RemoveTarget(character);
        }
    }
}