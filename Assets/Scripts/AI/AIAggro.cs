using System.Collections;
using System.Collections.Generic;
using Character;
using Damage;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    [RequireComponent(typeof(CharacterBase))]
    public class AIAggro : MonoBehaviour
    {
        private CharacterBase _character;
        private readonly Dictionary<CharacterBase, int> _damages = new();

        public UnityEvent<CharacterBase> aggroChanged;

        private void Awake()
        {
            _character = GetComponent<CharacterBase>();
            _damages.Clear();
        }

        private void OnEnable()
        {
            _character.damageReceived.AddListener(OnDamageReceived);
            StartCoroutine(CheckDamage_());
        }

        private void OnDisable()
        {
            _character.damageReceived.RemoveListener(OnDamageReceived);
            _damages.Clear();
        }
        
        private IEnumerator CheckDamage_()
        {
            while (enabled)
            {
                CharacterBase target = null;
                var maxDamage = 0;
                foreach (var (character, damage) in _damages)
                {
                    if (!character.isAlive) continue;

                    if (damage <= maxDamage) continue;
                    
                    maxDamage = damage;
                    target = character;
                }

                if (target != null)
                {
                    aggroChanged?.Invoke(target);
                }
                
                yield return new WaitForSeconds(1);
            }
        }

        private void OnDamageReceived(CharacterBase _, DamageHit damage)
        {
            if (!_character.isAlive || damage.damageType != DamageType.Health) return;

            var instigator = damage.instigator;
            if (instigator == null || !instigator.isAlive) return;
            
            instigator.dead.AddListener(RemoveTarget);
            
            if (!_damages.TryGetValue(instigator, out var value))
            {
                value = 0;
            }

            value += damage.value;

            _damages[damage.instigator] = value;
        }

        public void RemoveTarget(CharacterBase character)
        {
            character.dead.RemoveListener(RemoveTarget);
            _damages.Remove(character);
        }
    }
}