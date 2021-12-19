using System;
using System.Collections;
using AbilitySystem;
using Character;
using Damage;
using TargetSystem;
using UnityEngine;
using Weapon;

namespace Player.Abilities
{
    [CreateAssetMenu(fileName = "MeleeAttackAbility", menuName = "AbilitySystem/Abilities/MeleeAttack", order = 0)]
    public class MeleeAttackAbility : AbilityBase
    {
        public float range;
        public string animatorStateName;
        public float duration = 0.5f;
        public int weaponIndex;
        public float skillDamage;
        
        [NonSerialized]
        private DamageIntent _intent;

        protected override void OnActivate(AbilitySystemComponent source)
        {
            source.StartCoroutine(OnAttacking_(source));
        }
        
        private IEnumerator OnAttacking_(AbilitySystemComponent source)
        {
            var targetSystem = source.GetComponent<ITargetSystemInterface>();
            var character = source.GetComponent<CharacterBase>();
            var characterMovement = source.GetComponent<CharacterMovement>();
            var weaponMelee = source.GetComponent<IWeaponMeleeHandler>()?.GetWeaponMeleeController(weaponIndex);
            
            var target = targetSystem?.GetCurrentTarget();
            
            if (!target || !target.isValid || !weaponMelee || !character || !character.isAlive || !characterMovement)
            {
                Deactivate(source);
                yield break;
            }

            if (characterMovement.SetDestination(target.transform.position, range))
            {
                yield return new WaitWhile(() => character.isAlive && characterMovement.isNavigation && target && isActive);
            }

            if (!isActive || !target || !target.isValid || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }
            
            characterMovement.StopMovement();

            characterMovement.LookAt(target.transform);
            
            if(!isActive || !target || !target.isValid || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }

            _intent.damageType = DamageType.Health;
            _intent.skillDamage = skillDamage;
            _intent.source = character;
            
            weaponMelee.SetDamageIntent(_intent);
            
            character.TriggerAnimation(animatorStateName);
            yield return new WaitForSeconds(duration / character.animSpeed);
            character.StopAnimation();
            Deactivate(source);
        }

        protected override void OnDeactivate(AbilitySystemComponent source)
        {
        }
    }
}