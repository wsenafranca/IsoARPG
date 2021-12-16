using System;
using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Abilities;
using AttributeSystem;
using CombatSystem.Damage;
using UnityEngine;

namespace AbilitySystem
{
    [RequireComponent(typeof(AttributeSet))]
    public class AbilitySystemComponent : MonoBehaviour
    {
        [HideInInspector]
        public AttributeSet attributeSet;
        
        [SerializeField]
        private List<AbilityBase> abilities;

        [SerializeField] private Transform damageLocation;
        
        private readonly Queue<DamageInfo> _damages = new();
        
        public event Action<AbilityBase, float> OnAbilityActivated;
        public event Action<AbilityBase> OnAbilityDeactivated;
        
        public bool isAnyAbilityActive => abilities.Count((ability) => ability.isActive) > 0;

        private void Awake()
        {
            attributeSet = GetComponent<AttributeSet>();
        }

        private void OnEnable()
        {
            DeactivateAllAbilities();
        }

        private void LateUpdate()
        {
            while (_damages.TryDequeue(out var damage))
            {
                OnApplyDamage(damage);
            }
        }
        
        public int IndexOfAbility<T>() where T : AbilityBase
        {
            return abilities.FindIndex((ability) => ability.GetType() == typeof(T));
        }
        
        public void AddAbility(AbilityBase ability)
        {
            if (abilities.Contains(ability)) return;
            
            abilities.Add(ability);
        }

        public void RemoveAbility(AbilityBase ability)
        {
            abilities.Remove(ability);
        }

        public void RemoveAbility<T>() where T : AbilityBase
        {
            abilities.RemoveAll((ability) => ability.GetType() == typeof(T));
        }
        
        public bool IsActiveAbility(int index)
        {
            return index >= 0 && abilities[index].isActive;
        }

        public AbilityActivateResult TryActivateAbility<T>() where T : AbilityBase
        {
            return TryActivateAbility(IndexOfAbility<T>());
        }
        
        public AbilityActivateResult TryActivateAbility(int index)
        {
            if (index < 0) return AbilityActivateResult.NotFound;

            var ret = abilities[index].CommitAbility(this);
            if (ret == AbilityActivateResult.Success)
            {
                OnAbilityActivated?.Invoke(abilities[index], abilities[index].cooldown);
            }
            
            return ret;
        }
        
        public void DeactivateAbility<T>() where T : AbilityBase
        {
            DeactivateAbility(IndexOfAbility<T>());
        }

        public void DeactivateAbility(int index)
        {
            if (index < 0 || !abilities[index].Deactivate(this)) return;
            
            OnAbilityDeactivated?.Invoke(abilities[index]);
        }

        public void DeactivateAllAbilities()
        {
            foreach (var ability in abilities.Where(ability => ability.isActive))
            {
                ability.Deactivate(this);
            }
        }

        public void ApplyDamage(DamageIntent intent)
        {
        }

        public void ApplyDamage(DamageInfo damage)
        {
            _damages.Enqueue(damage);
        }
        
        private void OnApplyDamage(DamageInfo damage)
        {
            if (damage.blockedHit)
            {
                DamageOutputManager.instance.ShowText(damageLocation.position, "Block", Color.white);
                return;
            }
            
            if (damage.missedHit)
            {
                DamageOutputManager.instance.ShowText(damageLocation.position, "Miss", Color.gray);
                return;
            }

            if (damage.criticalHit)
            {
                DamageOutputManager.instance.ShowText(damageLocation.position, "Critical", Color.yellow);
            }
        }
    }
}