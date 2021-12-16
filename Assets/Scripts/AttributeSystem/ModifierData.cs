using System;

namespace AttributeSystem
{
    [Serializable]
    public struct AdditiveModifierData
    {
        public Attribute attribute;
        public int value;
    }
    
    [Serializable]
    public struct MultiplicativeModifierData
    {
        public Attribute attribute;
        public float value;
    }

    [Serializable]
    public struct AdditiveModifierDataRange
    {
        public Attribute attribute;
        public int minValue;
        public int maxValue;
    }
    
    [Serializable]
    public struct MultiplicativeModifierDataRange
    {
        public Attribute attribute;
        public float minValue;
        public float maxValue;
    }
}