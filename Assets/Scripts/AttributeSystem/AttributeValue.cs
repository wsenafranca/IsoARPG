using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AttributeSystem
{
    [Serializable]
    public class AttributeValue : BaseAttributeValue<int>
    {
        private readonly List<AdditiveAttributeModifier> _additiveList = new();
        private readonly List<MultiplicativeAttributeModifier> _multiplicativeList = new();

        private int _currentValue;
        private bool _needUpdate = true;
        
        public AttributeValue() : this(0) {}
        
        public AttributeValue(int baseValue = 0) : base(baseValue)
        {
            _currentValue = baseValue;
        }

        public int currentValue
        {
            get
            {
                if (!_needUpdate) return _currentValue;
                _needUpdate = false;

                var sumAdditive = _additiveList.Sum(modifier => modifier.baseValue);
                var sumMultiplicative = 1 + _multiplicativeList.Sum(modifier=>modifier.baseValue);

                _currentValue = Mathf.FloorToInt((baseValue + sumAdditive) * sumMultiplicative);
                
                return _currentValue;
            }
        }

        public void AddModifier(AdditiveAttributeModifier modifier)
        {
            _needUpdate = true;
            _additiveList.Add(modifier);
        }

        public void AddModifier(MultiplicativeAttributeModifier modifier)
        {
            _needUpdate = true;
            _multiplicativeList.Add(modifier);
        }

        public void RemoveModifier(AdditiveAttributeModifier modifier)
        {
            if (_additiveList.Remove(modifier))
            {
                _needUpdate = true;
            }
        }

        public void RemoveModifier(MultiplicativeAttributeModifier modifier)
        {
            if (_multiplicativeList.Remove(modifier))
            {
                _needUpdate = true;
            }
        }
        
        public void RemoveAllModifier(object source)
        {
            if (_additiveList.RemoveAll(modifier => modifier.source == source) > 0)
            {
                _needUpdate = true;
            }

            if (_multiplicativeList.RemoveAll(modifier => modifier.source == source) > 0)
            {
                _needUpdate = true;
            }
        }
    }
}