using System;
using AttributeSystem;

namespace Item
{
    [Serializable]
    public struct EquipmentItemLevelRange
    {
        public int level;
        public float probability;
    }

    public enum EquipmentItemRarity
    {
        Random,
        Common,
        Rare,
        Epic,
        Legendary,
        Unique
    }

    public enum EquipmentItemInstanceRarity
    {
        Common, // gray
        Rare, // green
        Epic, // purple
        Legendary, // gold
        Unique // red
    }
    
    [Serializable]
    public struct EquipmentItemAdditiveAttributeModifierProbabilityData
    {
        public AdditiveAttributeModifierDataRange bonus;
        public float probability;
    }
    
    [Serializable]
    public struct EquipmentItemMultiplicativeAttributeModifierProbabilityData
    {
        public MultiplicativeAttributeModifierDataRange bonus;
        public float probability;
    }
}