using System;
using UnityEngine;

namespace AbilitySystem
{
    public delegate void AttributeChangedDelegate(Attribute attribute, AttributeValue value);
    
    [Serializable]
    public class AttributeSet
    {
        [SerializeField] 
        private AttributeValue[] attributes;

        [NonSerialized] 
        private float _health;
        
        [NonSerialized] 
        private float _mana;
        
        [NonSerialized] 
        private float _magicShield;

        public event AttributeChangedDelegate OnAttributeChanged;

        public AttributeSet()
        {
            attributes = new AttributeValue[numAttributes];
            this[Attribute.MaxHealth] = new AttributeValue(1);
            this[Attribute.Strength] = new AttributeValue(1);
            this[Attribute.Stamina] = new AttributeValue(1);
            this[Attribute.Dexterity] = new AttributeValue(1);
            this[Attribute.Intelligence] = new AttributeValue(1);
            this[Attribute.AttackSpeed] = new AttributeValue(1);
            this[Attribute.MoveSpeed] = new AttributeValue(3);
        }

        public static int numAttributes => Enum.GetNames(typeof(Attribute)).Length;

        public void Init()
        {
            for (var i = 0; i < attributes.Length; i++)
            {
                this[(Attribute)i] = attributes[i];
            }
            Restore();
        }
        
        public void Restore()
        {
            SetHealth(this[Attribute.MaxHealth].GetCurrentValue());
            SetMana(this[Attribute.MaxMana].GetCurrentValue());
            SetMagicShield(this[Attribute.MaxEnergyShield].GetCurrentValue());
        }

        public float GetHealth()
        {
            return _health;
        }

        public void SetHealth(float value)
        {
            _health = value;
        }
        
        public float GetMana()
        {
            return _mana;
        }

        public void SetMana(float value)
        {
            _mana = value;
        }
        
        public float GetMagicShield()
        {
            return _magicShield;
        }

        public void SetMagicShield(float value)
        {
            _magicShield = value;
        }

        public float GetAttributeBaseValue(Attribute attribute)
        {
            return attributes[(int)attribute].GetBaseValue();
        }

        public void SetAttributeBaseValue(Attribute attribute, float value)
        {
            attributes[(int)attribute].SetBaseValue(value);
            OnAttributeChanged?.Invoke(attribute, attributes[(int)attribute]);
        }
        
        public float GetAttributeCurrentValue(Attribute attribute)
        {
            return attributes[(int)attribute].GetCurrentValue();
        }

        public void SetAttributeCurrentValue(Attribute attribute, float value)
        {
            attributes[(int)attribute].SetCurrentValue(value);
            OnAttributeChanged?.Invoke(attribute, attributes[(int)attribute]);
        }

        public AttributeValue this[Attribute attribute]
        {
            get => attributes[(int)attribute];
            private set
            {
                attributes[(int)attribute].SetBaseValue(value.GetBaseValue());
                attributes[(int)attribute].SetCurrentValue(value.GetBaseValue());
                OnAttributeChanged?.Invoke(attribute, attributes[(int)attribute]);
            }
        }

        public string DumpData()
        {
            var text = "";
            for (var i = 0; i < numAttributes; i++)
            {
                text += Enum.GetName(typeof(Attribute), i) + " = " + attributes[i] + "\n";
            }

            return text;
        }
    }
}