﻿using System;
using System.Collections;
using AbilitySystem;
using Character;
using Damage;
using Item;
using TargetSystem;
using UnityEngine;

namespace Player.Abilities
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
            var characterMovement = source.GetComponent<CharacterMovement>();
            var weaponMelee = source.GetComponent<IWeaponMeleeControllerHandler>()?.GetWeaponMeleeController(weaponIndex);
            
            var target = targetSystem?.GetCurrentTarget();
            
            if (!target || !weaponMelee || !character || !character.isAlive || !characterMovement)
            {
                Deactivate(source);
                yield break;
            }
            
            characterMovement.SetDestination(target.transform.position, range);
            yield return new WaitWhile(() => character.isAlive && characterMovement.isNavigation && target && isActive);

            if (!isActive || !target || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }
            
            characterMovement.StopMovement();

            characterMovement.LookAt(target.transform);
            
            if(!isActive || !target || !character.isAlive)
            {
                Deactivate(source);
                yield break;
            }

            _intent.damageType = DamageType.Health;
            _intent.source = character;
            
            weaponMelee.SetDamageIntent(_intent);
            
            character.PlayAnimation(animatorStateName+weaponIndex);
            yield return new WaitForSeconds(duration);
            character.StopAnimation();
            Deactivate(source);
        }

        protected override void OnDeactivate(AbilitySystemComponent source)
        {
        }
    }
}