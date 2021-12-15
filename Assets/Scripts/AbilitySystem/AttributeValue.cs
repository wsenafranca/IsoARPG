using System;
using UnityEngine;

namespace AbilitySystem
{
    [Serializable]
    public struct AttributeValue
    {
        public AttributeValue(float value)
        {
            baseValue = value;
            _currentValue = value;
        }
        
        [SerializeField]
        private float baseValue;
        
        [NonSerialized]
        private float _currentValue;

        public float GetCurrentValue()
        {
            return _currentValue;
        }

        public void SetCurrentValue(float value)
        {
            _currentValue = value;
        }
        
        public float GetBaseValue()
        {
            return baseValue;
        }

        public void SetBaseValue(float value)
        {
            baseValue = value;
        }

        public override string ToString()
        {
            return _currentValue + "(" + baseValue + ")";
        }
    }
}