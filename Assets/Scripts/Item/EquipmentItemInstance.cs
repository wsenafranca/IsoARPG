using System;
using System.Collections.Generic;
using AttributeSystem;
using InventorySystem;
using UnityEngine;

namespace Item
{
    [Serializable]
    public class EquipmentItemInstance : ItemInstance, IInventoryEquipmentItem
    {
        public List<AttributeData> attributes;
        
        public List<AdditiveAttributeModifierData> additiveModifiers;
        public List<MultiplicativeAttributeModifierData> multiplicativeModifiers;
        
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

        public EquipmentItemInstance(Guid guid) : base(guid) { }

        public EquipmentType equipmentType => ((EquipmentItem)itemBase).equipmentType;
        public IEnumerable<EquipmentVisualData> visualItemPrefab => ((EquipmentItem)itemBase).visualItemPrefab;
        public GameObject attachItemPrefab => ((EquipmentItem)itemBase).attachItemPrefab;
        public IEnumerable<EquipmentRequirementData> requirements => ((EquipmentItem)itemBase).requirements;
    }
}