using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        [SerializeField]
        private List<AbilityBase> abilities;

        [HideInInspector]
        public IAbilitySystemHandler handler;
        
        public UnityEvent<AbilityBase, float> abilityActivated;
        public UnityEvent<AbilityBase> abilityDeactivated;
        
        public bool isAnyAbilityActive => abilities.Count((ability) => ability.isActive) > 0;

        private void Awake()
        {
            handler = GetComponent<IAbilitySystemHandler>();
        }

        private void OnEnable()
        {
            DeactivateAllAbilities();
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

            if (handler == null) return AbilityActivateResult.NotFound;

            var ret = abilities[index].CommitAbility(this);
            if (ret == AbilityActivateResult.Success)
            {
                abilityActivated?.Invoke(abilities[index], abilities[index].cooldown);
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
            
            abilityDeactivated?.Invoke(abilities[index]);
        }

        public void DeactivateAllAbilities()
        {
            foreach (var ability in abilities.Where(ability => ability.isActive))
            {
                ability.Deactivate(this);
            }
        }
    }
}