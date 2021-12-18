using System;
using System.Collections.Generic;
using AttributeSystem;
using Damage;
using UnityEngine;
using Attribute = AttributeSystem.Attribute;

namespace Character
{
    [RequireComponent(typeof(AttributeSet))]
    [RequireComponent(typeof(Animator))]
    public class BaseCharacterController : MonoBehaviour
    {
        protected Animator animator;
        
        [HideInInspector]
        public AttributeSet attributeSet;
        
        [HideInInspector]
        public int health;
        private readonly Queue<DamageInfo> _damages = new(30);
        
        [HideInInspector]
        public int mana;
        
        [HideInInspector]
        public int energyShield;
        
        public bool isPlayingAnimation { get; private set; }
        public bool isAlive => health > 0;

        private static readonly int AnimSpeedHash = Animator.StringToHash("animSpeed");

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            attributeSet = GetComponent<AttributeSet>();
        }

        protected virtual void OnEnable()
        {
            _damages.Clear();
            health = attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth);
            mana = attributeSet.GetAttributeValueOrDefault(Attribute.MaxMana);
            energyShield = attributeSet.GetAttributeValueOrDefault(Attribute.MaxEnergyShield);
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Update()
        {
            animator.SetFloat(AnimSpeedHash, 1.0f);
        }

        private void LateUpdate()
        {
            while (_damages.TryDequeue(out var damage))
            {
                if (damage.value > 0)
                {
                    switch (damage.damageType)
                    {
                        case DamageType.Health:
                            health = (int)Mathf.Clamp(health - damage.value, 0.0f, attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth));
                            break;
                        case DamageType.Mana:
                            mana = (int)Mathf.Clamp(mana - damage.value, 0.0f, attributeSet.GetAttributeValueOrDefault(Attribute.MaxMana));
                            break;
                        case DamageType.EnergyShield:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                DamageOutputManager.instance.ShowDamage(damage);
            }
        }
        
        public void ApplyDamage(DamageIntent intent)
        {
            // damages are applied on LateUpdate
            _damages.Enqueue(DamageCalculator.CalculateDamage(intent, this));
        }

        public void PlayAnimation(string stateName)
        {
            if (isPlayingAnimation) return;
        
            animator.CrossFade(stateName, 0.1f);
            isPlayingAnimation = true;
        }

        public void StopAnimation()
        {
            isPlayingAnimation = false;
        }
    }
}