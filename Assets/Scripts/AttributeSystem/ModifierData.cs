using System;

namespace AttributeSystem
{
    [Serializable]
    public struct AdditiveModifierData
    {
        public Attribute attribute;
        public AdditiveAttributeModifier value;
    }
    
    [Serializable]
    public struct MultiplicativeModifierData
    {
        public Attribute attribute;
        public AdditiveAttributeModifier value;
    }
}