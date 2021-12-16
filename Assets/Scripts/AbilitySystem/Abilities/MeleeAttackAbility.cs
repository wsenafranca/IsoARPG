using System;
using System.Collections;
using CombatSystem.Damage;
using Controller;
using Item;
using TargetSystem;
using UnityEngine;

namespace AbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "MeleeAttackAbility", menuName = "AbilitySystem/Abilities/MeleeAttack", order = 0)]
    public class MeleeAttackAbility : AbilityBase
    {
        public float range;
        public string animatorStateName;
        public float duration = 0.5f;
        public int weaponIndex;
        
        [NonSerialized]
        private DamageIntent _intent;

        protected override void OnActivate(AbilitySystemComponent source)
        {
            source.StartCoroutine(OnAttacking_(source));
        }
        
        private IEnumerator OnAttacking_(AbilitySystemComponent source)
        {
            var targetSystem = source.GetComponent<ITargetSystemInterface>();
            var character = source.GetComponent<BaseCharacterController>();
            var weaponMelee = source.GetComponent<IWeaponMeleeControllerInterface>()?.GetWeaponMeleeController(weaponIndex);
            
            var target = targetSystem?.GetCurrentTarget();
            
            if (!target || !weaponMelee || !character || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }
            
            character.SetDestination(target.transform.position, range);
            yield return new WaitWhile(() => character.isAlive && character.isNavigation && target && isActive);

            if (!isActive || !target || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }
            
            character.StopMovement();

            character.LookAt(target.transform);
            yield return new WaitWhile(() => character.isLookingAtTarget);
            
            if(!isActive || !target || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }

            _intent.damageTarget = DamageTarget.Health;
            _intent.source = source.gameObject;
            
            weaponMelee.SetDamageIntent(_intent);
            
            character.PlayAnimation(animatorStateName);
            yield return new WaitForSeconds(duration);
            character.StopAnimation();
            Deactivate(source);
        }

        protected override void OnDeactivate(AbilitySystemComponent source)
        {
        }
    }
}