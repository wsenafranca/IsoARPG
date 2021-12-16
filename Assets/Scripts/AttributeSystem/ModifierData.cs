using System;

namespace AttributeSystem
{
    [Serializable]
    public struct AdditiveAttributeModifierData
    {
        public Attribute attribute;
        public int value;
    }
    
    [Serializable]
    public struct MultiplicativeAttributeModifierData
    {
        public Attribute attribute;
        public float value;
    }

    [Serializable]
    public struct AdditiveAttributeModifierDataRange
    {
        public Attribute attribute;
        public int minValue;
        public int maxValue;
    }
    
    [Serializable]
    public struct MultiplicativeAttributeModifierDataRange
    {
        public Attribute attribute;
        public float minValue;
        public float maxValue;
    }
}