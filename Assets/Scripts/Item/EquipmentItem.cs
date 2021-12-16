using System;
using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Abilities;
using AttributeSystem;
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

        public EquipmentType type;

        public EquipmentItemRarity rarity;
        
        public List<EquipmentRequirementData> requirements;

        public List<EquipmentLevelRange> level;

        public List<AdditiveModifierDataRange> additiveModifiers;
        public List<MultiplicativeModifierDataRange> multiplicativeModifiers;
        
        public List<AbilityBase> grantedAbilities;

        public bool isWeapon => type is EquipmentType.OneHandWeapon or EquipmentType.TwoHandWeapon;

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

            var additiveModifierList = (from mod in additiveModifiers 
                let minValue = Mathf.Min(mod.minValue, mod.maxValue) 
                let maxValue = Mathf.Max(mod.minValue, mod.maxValue) 
                let value = Mathf.FloorToInt(Mathf.Lerp(minValue, maxValue, a)) 
                select new AdditiveModifierData { attribute = mod.attribute, value = value }).ToList();
            
            var multiplicativeModifierList = (from mod in multiplicativeModifiers 
                let minValue = Mathf.Min(mod.minValue, mod.maxValue) 
                let maxValue = Mathf.Max(mod.minValue, mod.maxValue) 
                let value = Mathf.FloorToInt(Mathf.Lerp(minValue, maxValue, a)) 
                select new MultiplicativeModifierData { attribute = mod.attribute, value = value }).ToList();

            var p = Random.value;
            var itemLevel = (from i in level.OrderByDescending((i => i.probability)) where i.probability > p select i.level).FirstOrDefault();
            
            return new EquipmentItemInstance
            {
                guid = Guid.NewGuid(),
                itemBase = this,
                additiveModifiers = additiveModifierList,
                multiplicativeModifiers = multiplicativeModifierList,
                level = itemLevel,
                rarity =  r
            };
        }
    }
}