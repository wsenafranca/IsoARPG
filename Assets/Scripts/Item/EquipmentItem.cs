﻿using System;
using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Abilities;
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

        [Header("Stats")]
        public List<AdditiveAttributeModifierDataRange> statsAdditiveModifiers;
        public List<MultiplicativeAttributeModifierDataRange> statsMultiplicativeModifiers;
        
        [Header("Bonus")]
        public List<EquipmentItemAdditiveAttributeModifierBonusData> bonusAdditiveModifiers;
        public List<EquipmentItemMultiplicativeAttributeModifierBonusData> bonusMultiplicativeModifiers;
        
        public List<AbilityBase> grantedAbilities;

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

            var statsAdditiveModifierList = (from mod in statsAdditiveModifiers 
                let minValue = Mathf.Min(mod.minValue, mod.maxValue) 
                let maxValue = Mathf.Max(mod.minValue, mod.maxValue) 
                let value = Mathf.FloorToInt(Mathf.Lerp(minValue, maxValue, a)) 
                select new AdditiveAttributeModifierData { attribute = mod.attribute, value = value }).ToList();
            
            var statsMultiplicativeModifierList = (from mod in statsMultiplicativeModifiers 
                let minValue = Mathf.Min(mod.minValue, mod.maxValue) 
                let maxValue = Mathf.Max(mod.minValue, mod.maxValue) 
                let value = Mathf.Lerp(minValue, maxValue, a) 
                select new MultiplicativeAttributeModifierData { attribute = mod.attribute, value = value }).ToList();

            var bonusAdditiveModifierList = new List<AdditiveAttributeModifierData>();
            foreach (var data in bonusAdditiveModifiers)
            {
                if(Random.value > Mathf.Lerp(data.probability, 1.0f, a)) continue;

                var minValue = Mathf.Min(data.bonus.minValue, data.bonus.maxValue);
                var maxValue = Mathf.Max(data.bonus.minValue, data.bonus.maxValue);
                var value = Mathf.FloorToInt(Mathf.Lerp(minValue, maxValue, a));
                bonusAdditiveModifierList.Add(new AdditiveAttributeModifierData{attribute = data.bonus.attribute, value = value});
            }
            
            var bonusMultiplicativeModifierList = new List<MultiplicativeAttributeModifierData>();
            foreach (var data in bonusMultiplicativeModifiers)
            {
                if(Random.value > Mathf.Lerp(data.probability, 1.0f, a)) continue;

                var minValue = Mathf.Min(data.bonus.minValue, data.bonus.maxValue);
                var maxValue = Mathf.Max(data.bonus.minValue, data.bonus.maxValue);
                var value = Mathf.Lerp(minValue, maxValue, a);
                bonusMultiplicativeModifierList.Add(new MultiplicativeAttributeModifierData{attribute = data.bonus.attribute, value = value});
            }

            var itemLevel = (from i in level.OrderByDescending((i => i.probability)) where i.probability > Random.value select i.level).FirstOrDefault();
            
            return new EquipmentItemInstance(Guid.NewGuid())
            {
                itemBase = this,
                statsAdditiveModifiers = statsAdditiveModifierList,
                statsMultiplicativeModifiers = statsMultiplicativeModifierList,
                bonusAdditiveModifiers = bonusAdditiveModifierList,
                bonusMultiplicativeModifiers = bonusMultiplicativeModifierList,
                level = itemLevel,
                rarity =  r
            };
        }
    }
}