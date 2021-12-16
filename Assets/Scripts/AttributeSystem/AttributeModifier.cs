using System;

namespace AttributeSystem
{
    [Serializable]
    public abstract class BaseAttributeModifier<T>
    {
        public T baseValue;
        public object source { get; private set; }
        
        protected BaseAttributeModifier(T baseValue, object source)
        {
            this.baseValue = baseValue;
            this.source = source;
        }
    }
    
    [Serializable]
    public class AdditiveAttributeModifier : BaseAttributeModifier<int>
    {
        public AdditiveAttributeModifier(int baseValue, object source) : base(baseValue, source) {}
        public AdditiveAttributeModifier() : this(0, null) {}
    }

    [Serializable]
    public class MultiplicativeAttributeModifier : BaseAttributeModifier<float>
    {
        public MultiplicativeAttributeModifier(float baseValue, object source) : base(baseValue, source) {}
        public MultiplicativeAttributeModifier() : this(0, null) {}
    }
}