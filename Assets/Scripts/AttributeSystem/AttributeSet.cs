using System.Collections.Generic;
using UnityEngine;

namespace AttributeSystem
{
    public class AttributeSet : MonoBehaviour
    {
        [SerializeField]
        private List<AttributeData> attributes;

        [HideInInspector]
        public int health;
        
        [HideInInspector]
        public int mana;
        
        [HideInInspector]
        public int energyShield;

        private void Start()
        {
            health = GetAttributeValueOrDefault(Attribute.MaxHealth);
            mana = GetAttributeValueOrDefault(Attribute.MaxMana);
            energyShield = GetAttributeValueOrDefault(Attribute.MaxEnergyShield);
        }
        
        public bool TryGetAttribute(Attribute attribute, out AttributeValue value)
        {
            value = attributes.Find((data => data.attribute == attribute)).value;
            return value != null;
        }

        public int GetAttributeValueOrDefault(Attribute attribute) => attributes.Find((data => data.attribute == attribute)).value?.currentValue ?? 0;
    }
}