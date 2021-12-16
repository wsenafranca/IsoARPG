using System;
using System.Collections.Generic;
using AttributeSystem;
using UnityEngine;

namespace Item
{
    public enum ItemCategory
    {
        Consumable,
        Equipment,
        QuestItem
    }
    
    public enum EquipmentSlot
    {
        Helm = 0,
        Armor,
        Pants,
        Gloves,
        Boots,
        MainHand,
        OffHand,
        Necklace,
        Ring1,
        Ring2,
        Cape,
        Pet
    }

    [Serializable]
    public struct EquipmentSlotSocket
    {
        public Transform mainHand;
        public Transform offHand;
        public Transform helm;
        public Transform armor;
        public Transform pants;
        public Transform gloves;
        public Transform boots;
        public Transform necklace;
        public Transform ring1;
        public Transform ring2;
    }

    [Serializable]
    public struct EquipmentLevelRange
    {
        public int level;
        public float probability;
    }

    public enum EquipmentRequirement
    {
        Level,
        Strength,
        Stamina,
        Dexterity,
        Intelligence
    }

    [Serializable]
    public struct EquipmentRequirementData
    {
        public EquipmentRequirement requirement;
        public int value;
    }
    
    public enum EquipmentType
    {
        OneHandWeapon,
        TwoHandWeapon,
        Shield,
        Helm,
        Armor,
        Pants,
        Gloves,
        Boots,
        Necklace,
        Ring,
        Cape,
        Pet
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
    
    public enum EquipmentVisualSlot : byte
    {
        Head = 0,
        Eyebrow,
        Hair,
        Torso,
        UpperArmLeft,
        LowerArmLeft,
        HandLeft,
        UpperArmRight,
        LowerArmRight,
        HandRight,
        Hips,
        LegLeft,
        LegRight
    }

    [Serializable]
    public class EquipmentVisualData
    {
        public EquipmentVisualSlot slot;
        public GameObject prefab;
    }

    [Serializable]
    public class ItemInstance : IEquatable<ItemInstance>
    {
        public Guid guid;
        public ItemBase itemBase;

        public T GetItemBase<T>() where T : ItemBase => itemBase as T;
        
        public virtual Color itemColor => Color.white;
        public bool Equals(ItemInstance other)
        {
            return other != null && guid == other.guid;
        }
    }
    
    [Serializable]
    public class EquipmentItemInstance : ItemInstance
    {
        public List<AdditiveModifierData> additiveModifiers;
        public List<MultiplicativeModifierData> multiplicativeModifiers;
        public int level;
        public EquipmentItemInstanceRarity rarity;
        public byte durability = 255;

        public override Color itemColor => rarity switch
        {
            EquipmentItemInstanceRarity.Common => GameAsset.instance.commonItem,
            EquipmentItemInstanceRarity.Rare => GameAsset.instance.rateItem,
            EquipmentItemInstanceRarity.Epic => GameAsset.instance.epicItem,
            EquipmentItemInstanceRarity.Legendary => GameAsset.instance.legendaryItem,
            EquipmentItemInstanceRarity.Unique => GameAsset.instance.uniqueItem,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}