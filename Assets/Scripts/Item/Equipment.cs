using System;
using System.Collections.Generic;
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
    public struct EquipmentItemAdditiveAttributeModifierBonusData
    {
        public AdditiveAttributeModifierDataRange bonus;
        public float probability;
    }
    
    [Serializable]
    public struct EquipmentItemMultiplicativeAttributeModifierBonusData
    {
        public MultiplicativeAttributeModifierDataRange bonus;
        public float probability;
    }
}