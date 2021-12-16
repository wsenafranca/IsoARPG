using System;
using System.Collections.Generic;
using System.Linq;
using AttributeSystem;
using InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Item
{
    [CreateAssetMenu(fileName = "EquipmentItem", menuName = "InventorySystem/Item/Equipment", order = 0)]
    public class EquipmentItem : ItemBase
    {
        [Header("Equipment")]
        public EquipmentVisualData[] visualItemPrefab;

        public GameObject attachItemPrefab;

        public EquipmentType equipmentType;

        public EquipmentItemRarity rarity;
        
        public List<EquipmentRequirementData> requirements;

        public List<EquipmentItemLevelRange> level;

        public List<AttributeData> attributes;
        
        public List<EquipmentItemAdditiveAttributeModifierProbabilityData> additiveModifiers;
        public List<EquipmentItemMultiplicativeAttributeModifierProbabilityData> multiplicativeModifiers;
        
        public int GetRequirements(EquipmentRequirement requirement)
        {
            return requirements.Where(req => req.requirement == requirement).Select(req => req.value).FirstOrDefault();
        }
        
        public override ItemInstance CreateItemInstance()
        {
            var r = rarity switch
            {
                EquipmentItemRarity.Random => Random.value switch
                {
                    > 0.99f => EquipmentItemInstanceRarity.Unique,
                    > 0.95f => EquipmentItemInstanceRarity.Legendary,
                    > 0.85f => EquipmentItemInstanceRarity.Epic,
                    > 0.65f => EquipmentItemInstanceRarity.Rare,
                    _ => EquipmentItemInstanceRarity.Common
                },
                EquipmentItemRarity.Common => EquipmentItemInstanceRarity.Common,
                EquipmentItemRarity.Rare => EquipmentItemInstanceRarity.Rare,
                EquipmentItemRarity.Epic => EquipmentItemInstanceRarity.Epic,
                EquipmentItemRarity.Legendary => EquipmentItemInstanceRarity.Legendary,
                EquipmentItemRarity.Unique => EquipmentItemInstanceRarity.Unique,
                _ => throw new ArgumentOutOfRangeException()
            };

            var a = r switch
            {
                EquipmentItemInstanceRarity.Common => Random.Range(0.0f, 1.0f),
                EquipmentItemInstanceRarity.Rare => Random.Range(0.5f, 1.0f),
                EquipmentItemInstanceRarity.Epic => Random.Range(0.85f, 1.0f),
                EquipmentItemInstanceRarity.Legendary => Random.Range(0.95f, 1.0f),
                EquipmentItemInstanceRarity.Unique => 1,
                _ => throw new ArgumentOutOfRangeException()
            };

            var additiveModifierList = new List<AdditiveAttributeModifierData>();
            foreach (var data in additiveModifiers)
            {
                if(Random.value > data.probability + a) continue;

                var minValue = Mathf.Min(data.bonus.minValue, data.bonus.maxValue);
                var maxValue = Mathf.Max(data.bonus.minValue, data.bonus.maxValue);
                var value = Mathf.FloorToInt(Mathf.Lerp(minValue, maxValue, a));
                additiveModifierList.Add(new AdditiveAttributeModifierData{attribute = data.bonus.attribute, value = value});
            }
            
            var multiplicativeModifierList = new List<MultiplicativeAttributeModifierData>();
            foreach (var data in multiplicativeModifiers)
            {
                if(Random.value > data.probability + a) continue;

                var minValue = Mathf.Min(data.bonus.minValue, data.bonus.maxValue);
                var maxValue = Mathf.Max(data.bonus.minValue, data.bonus.maxValue);
                var value = Mathf.Lerp(minValue, maxValue, a);
                multiplicativeModifierList.Add(new MultiplicativeAttributeModifierData{attribute = data.bonus.attribute, value = value});
            }

            var itemLevel = (from i in level.OrderByDescending((i => i.probability)) where Random.value < i.probability select i.level).FirstOrDefault();
            
            var itemAttributes = new List<AttributeData>();
            foreach (var attr in attributes)
            {
                var newAttr = attr;
                newAttr.value = new AttributeValue(attr.value.baseValue);
                itemAttributes.Add(newAttr);
            }

            var itemDurability = (byte)(Random.value * 200 + 25);
            
            var item = new EquipmentItemInstance(Guid.NewGuid())
            {
                itemBase = this,
                attributes = itemAttributes,
                additiveModifiers = additiveModifierList,
                multiplicativeModifiers = multiplicativeModifierList,
                level = itemLevel,
                durability = itemDurability,
                rarity =  r
            };
            
            if (itemLevel > 0)
            {
                foreach (var attr in item.attributes)
                {
                    attr.value.AddModifier(new AdditiveAttributeModifier(Mathf.FloorToInt(attr.value.baseValue * 0.2f + itemLevel * 2), item));
                }
            }
            
            return item;
        }
    }
}