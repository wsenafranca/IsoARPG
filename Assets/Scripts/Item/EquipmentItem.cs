using System;
using System.Collections.Generic;
using System.Linq;
using AbilitySystem;
using AbilitySystem.Abilities;
using AbilitySystem.Effects;
using UnityEngine;
using Attribute = AbilitySystem.Attribute;
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
        
        public List<EquipmentItemRequirement> requirements;

        public List<EquipmentLevelRange> level;

        public List<EquipmentItemBonusRange> bonus;
        
        public List<EffectBase> effects;
        
        public List<AbilityBase> grantedAbilities;

        public bool isWeapon => type is EquipmentType.OneHandWeapon or EquipmentType.TwoHandWeapon;

        public int GetRequirements(Attribute attribute)
        {
            return requirements.Where(req => req.attribute == attribute).Select(req => req.value).FirstOrDefault();
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

            var bonusList = new List<EquipmentItemBonus>();
            foreach (var b in bonus)
            {
                var minValue = Mathf.Min(b.minValue, b.maxValue);
                var maxValue = Mathf.Max(b.minValue, b.maxValue);
                var value = Mathf.Lerp(minValue, maxValue, a);
                switch (b.attribute)
                {
                    case Attribute.MaxHealth:
                    case Attribute.MaxMana:
                    case Attribute.Strength:
                    case Attribute.Stamina:
                    case Attribute.Dexterity:
                    case Attribute.Intelligence:
                    case Attribute.MinAttackPower:
                    case Attribute.MaxAttackPower:
                    case Attribute.AttackSpeed:
                    case Attribute.MaxEnergyShield:
                        value = b.modifer == EffectModifier.Multiplicative ? value : Mathf.CeilToInt(value);
                        break;
                    case Attribute.DefensePower:
                    case Attribute.EvasionRate:
                    case Attribute.CriticalHitRate:
                    case Attribute.CriticalHitDamage:
                    case Attribute.BlockRate:
                    case Attribute.MoveSpeed:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                bonusList.Add(new EquipmentItemBonus {attribute = b.attribute, modifier = b.modifer, value = value});
            }

            var p = Random.value;
            var itemLevel = (from i in level.OrderByDescending((i => i.probability)) where i.probability > p select i.level).FirstOrDefault();
            
            return new EquipmentItemInstance
            {
                guid = Guid.NewGuid(),
                itemBase = this,
                bonus = bonusList,
                level = itemLevel,
                rarity =  r
            };
        }
    }
}