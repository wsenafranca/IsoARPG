using System;
using System.Collections;
using System.Collections.Generic;
using AttributeSystem;
using Damage;
using UnityEngine;
using UnityEngine.Events;
using Attribute = AttributeSystem.Attribute;

namespace Character
{
    [RequireComponent(typeof(AttributeSet))]
    [RequireComponent(typeof(CharacterAnimator))]
    public class CharacterBase : MonoBehaviour
    {
        public string displayName;
        
        private CharacterAnimator _animator;
        
        [HideInInspector]
        public AttributeSet attributeSet;
        
        private readonly Queue<DamageInfo> _damages = new(30);

        public UnityEvent<CharacterBase> death;
        public UnityEvent<CharacterBase, int, int> currentHealthChanged;
        public UnityEvent<CharacterBase, int, int> currentManaChanged;
        public UnityEvent<CharacterBase, int, int> currentEnergyShieldChanged;

        [NonSerialized]
        private int _health;
        public int currentHealth
        {
            get => _health;
            set
            {
                if (value == _health) return;
                _health = value;
                currentHealthChanged?.Invoke(this, _health, attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth));
                
                if(!isAlive) death?.Invoke(this);
            }
        }

        [NonSerialized]
        private int _mana;
        public int currentMana
        {
            get => _mana;
            set
            {
                if (value == _mana) return;
                _mana = value;
                currentManaChanged?.Invoke(this, _mana, attributeSet.GetAttributeValueOrDefault(Attribute.MaxMana));
            }
        }
        
        [NonSerialized]
        private int _energyShield;
        public int currentEnergyShield
        {
            get => _energyShield;
            set
            {
                if (value == _energyShield) return;
                _energyShield = value;
                currentEnergyShieldChanged?.Invoke(this, _energyShield, attributeSet.GetAttributeValueOrDefault(Attribute.MaxEnergyShield));
            }
        }
        public bool isAlive => currentHealth > 0;

        public float animSpeed => Mathf.Clamp(0.7f + attributeSet.GetAttributeValueOrDefault(Attribute.AttackSpeed) / 300.0f, 0.8f, 3.0f);

        private void Awake()
        {
            _animator = GetComponent<CharacterAnimator>();
            attributeSet = GetComponent<AttributeSet>();
        }

        private void OnEnable()
        {
            currentHealth = attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth);
            currentMana = attributeSet.GetAttributeValueOrDefault(Attribute.MaxMana);
            currentEnergyShield = attributeSet.GetAttributeValueOrDefault(Attribute.MaxEnergyShield);

            _damages.Clear();
            StartCoroutine(ApplyDamage_());
        }

        private void Update()
        {
            _animator.animSpeed = animSpeed;
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
                            currentHealth = (int)Mathf.Clamp(currentHealth - damage.value, 0.0f, attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth));
                            break;
                        case DamageType.Mana:
                            currentMana = (int)Mathf.Clamp(currentMana - damage.value, 0.0f, attributeSet.GetAttributeValueOrDefault(Attribute.MaxMana));
                            break;
                        case DamageType.EnergyShield:
                            currentEnergyShield = (int)Mathf.Clamp(currentEnergyShield - damage.value, 0.0f, attributeSet.GetAttributeValueOrDefault(Attribute.MaxEnergyShield));
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
    }
}