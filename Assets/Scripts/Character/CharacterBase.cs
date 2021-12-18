using System;
using System.Collections;
using System.Collections.Generic;
using AttributeSystem;
using Damage;
using UnityEngine;
using Attribute = AttributeSystem.Attribute;

namespace Character
{
    [RequireComponent(typeof(AttributeSet))]
    [RequireComponent(typeof(Animator))]
    public class CharacterBase : MonoBehaviour
    {
        private Animator _animator;
        
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

        public float animSpeed => Mathf.Clamp(1 + attributeSet.GetAttributeValueOrDefault(Attribute.AttackSpeed) / 100.0f, 1.0f, 3.0f);
        
        private static readonly int AnimSpeedHash = Animator.StringToHash("animSpeed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            attributeSet = GetComponent<AttributeSet>();
        }

        private void OnEnable()
        {
            health = attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth);
            mana = attributeSet.GetAttributeValueOrDefault(Attribute.MaxMana);
            energyShield = attributeSet.GetAttributeValueOrDefault(Attribute.MaxEnergyShield);

            _damages.Clear();
            StartCoroutine(ApplyDamage_());
        }

        private void Update()
        {
            _animator.SetFloat(AnimSpeedHash, animSpeed);
        }

        private IEnumerator ApplyDamage_()
        {
            while (enabled)
            {
                yield return new WaitUntil(() => _damages.Count > 0 || !enabled);

                while (_damages.TryDequeue(out var damage))
                {
                    if (!isAlive)
                    {
                        _damages.Clear();
                        break;
                    }
                    
                    if (damage.value <= 0) continue;
                    
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
                        
                    DamageOutputManager.instance.ShowDamage(damage);
                }
            }
            
            _damages.Clear();
        }
        
        public void ApplyDamage(DamageIntent intent)
        {
            // damages are applied on LateUpdate
            _damages.Enqueue(DamageCalculator.CalculateDamage(intent, this));
        }

        public void TriggerAnimation(string stateName)
        {
            if (isPlayingAnimation) return;
        
            _animator.SetTrigger(stateName);
            isPlayingAnimation = true;
        }

        public void StopAnimation()
        {
            isPlayingAnimation = false;
        }
    }
}