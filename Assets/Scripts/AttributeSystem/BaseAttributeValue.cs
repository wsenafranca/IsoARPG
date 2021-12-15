using System;
using UnityEngine;

namespace AttributeSystem
{
    [Serializable]
    public abstract class BaseAttributeValue<T> where T : new()
    {
        [SerializeField]
        public T baseValue;

        protected BaseAttributeValue(T baseValue)
        {
            this.baseValue = baseValue;
        }
    }
}