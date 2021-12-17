using System.Collections;
using AbilitySystem;
using Controller;
using Item;
using TargetSystem;
using UnityEngine;

namespace Player.Abilities
{
    [CreateAssetMenu(fileName = "CollectAbility", menuName = "AbilitySystem/Abilities/Collect", order = 0)]
    public class CollectAbility : AbilityBase
    {
        public float range = 1.5f;

        protected override void OnActivate(AbilitySystemComponent source)
        {
            source.StartCoroutine(OnCollecting_(source));
        }
        
        private IEnumerator OnCollecting_(AbilitySystemComponent source)
        {
            var targetSystem = source.GetComponent<ITargetSystemInterface>();
            var character = source.GetComponent<BaseCharacterController>();
            
            var collectible = targetSystem?.GetCurrentTarget().GetComponent<Collectible>();
            
            if (!collectible || !character || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }
            
            character.SetDestination(collectible.transform.position, range);
            yield return new WaitWhile(() => character.isAlive && character.isNavigation && collectible && isActive);
            
            if (!isActive || !collectible || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }
            
            character.StopMovement();
            
            collectible.Collect(source.gameObject);
            
            Deactivate(source);
        }

        protected override void OnDeactivate(AbilitySystemComponent source)
        {
            
        }
    }
}