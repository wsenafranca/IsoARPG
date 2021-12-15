using System;
using System.Linq;
using AbilitySystem;
using InventorySystem;
using Item;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Attribute = AbilitySystem.Attribute;

namespace UI
{
    public class ItemTooltipView : MonoBehaviour
    {
        public static ItemTooltipView instance { get; private set; }

        public Canvas canvas;
        
        private TextMeshProUGUI _itemName;
        private RawImage _itemIcon;
        private TextMeshProUGUI _itemInstanceInfo;
        private TextMeshProUGUI _equipmentStats;
        private TextMeshProUGUI _equipmentRequirements;
        private TextMeshProUGUI _itemDescription;
        private TextMeshProUGUI _itemEffects;

        private void Awake()
        {
            _itemName = transform.Find("Header").transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            _itemIcon = transform.Find("Header").transform.Find("ItemIcon").GetComponent<RawImage>();
            _itemInstanceInfo = transform.Find("ItemInstanceInfo").GetComponent<TextMeshProUGUI>();
            _equipmentStats = transform.Find("EquipmentStats").GetComponent<TextMeshProUGUI>();
            _equipmentRequirements = transform.Find("EquipmentRequirements").GetComponent<TextMeshProUGUI>();
            _itemDescription = transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>();
            _itemEffects = transform.Find("ItemEffects").GetComponent<TextMeshProUGUI>();
            
            gameObject.SetActive(false);
            
            instance = this;
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowItem(InventoryController inventory, ItemInstance item, Vector2 screenPosition, Camera cam)
        {
            if (item == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _itemName.text = GetItemName(item);
            _itemIcon.texture = GetIcon(item);
            _itemInstanceInfo.text = GetItemInstanceInfo(item as EquipmentItemInstance);
            _equipmentStats.text = GetEquipmentStats(item as EquipmentItemInstance);
            _equipmentRequirements.text = GetEquipmentsRequirements(inventory, item);
            _itemDescription.text = GetItemDescription(item);
            _itemEffects.text = GetItemEffect(item);

            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>())
            {
                var textRectTransform = text.rectTransform;
                var textSize = textRectTransform.sizeDelta;
                var separator = transform.Find(text.gameObject.name + "Separator");
                textSize.y = text.preferredHeight;
                if (textSize.y > 0)
                {
                    textSize.y += 4;
                    textRectTransform.sizeDelta = textSize;
                    text.gameObject.SetActive(true);
                    if(separator) separator.gameObject.SetActive(true);
                }
                else
                {
                    text.gameObject.SetActive(false);
                    if(separator) separator.gameObject.SetActive(false);
                }
            }

            var rectTransform = GetComponent<RectTransform>();

            var parentRectTransform = transform.parent as RectTransform;
            if (parentRectTransform == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPosition, cam, out var parentPoint);
            rectTransform.localPosition = new Vector3(parentPoint.x, parentPoint.y, -300.0f) - parentRectTransform.localPosition;
            
            var rect = rectTransform.rect;
            var canvasRect = ((RectTransform)canvas.transform).rect;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            
            var clamped = rectTransform.anchoredPosition;
            clamped.x = Mathf.Clamp(clamped.x, 8.0f, canvasRect.width - rect.width - 8.0f);
            clamped.y = Mathf.Clamp(clamped.y, rect.height + 8.0f, canvasRect.height - rect.height - 8.0f);
            rectTransform.anchoredPosition = clamped;
            
            gameObject.SetActive(true);
        }

        private static string GetItemName(ItemInstance item)
        {
            if (item is EquipmentItemInstance { level: > 0 } equipmentItem)
            {
                return $"<color=#{ColorUtility.ToHtmlStringRGBA(item.itemColor)}>{item.itemBase.displayName} +{equipmentItem.level}</color>";
            }

            return $"<color=#{ColorUtility.ToHtmlStringRGBA(item.itemColor)}>{item.itemBase.displayName}</color>";
        }

        private static Texture2D GetIcon(ItemInstance item)
        {
            return item.itemBase.category switch
            {
                ItemCategory.Consumable => GameAsset.instance.categoryConsumable,
                ItemCategory.QuestItem => GameAsset.instance.categoryQuestItem,
                ItemCategory.Equipment => item.GetItemBase<EquipmentItem>().type switch
                {
                    EquipmentType.OneHandWeapon => GameAsset.instance.categoryEquipmentOneHandWeapon,
                    EquipmentType.TwoHandWeapon => GameAsset.instance.categoryEquipmentTwoHandWeapon,
                    EquipmentType.Shield => GameAsset.instance.categoryEquipmentShield,
                    EquipmentType.Helm => GameAsset.instance.categoryEquipmentHelm,
                    EquipmentType.Armor => GameAsset.instance.categoryEquipmentArmor,
                    EquipmentType.Pants => GameAsset.instance.categoryEquipmentPants,
                    EquipmentType.Gloves => GameAsset.instance.categoryEquipmentGloves,
                    EquipmentType.Boots => GameAsset.instance.categoryEquipmentBoots,
                    EquipmentType.Necklace => GameAsset.instance.categoryEquipmentNecklace,
                    EquipmentType.Ring => GameAsset.instance.categoryEquipmentRing,
                    EquipmentType.Cape => GameAsset.instance.categoryEquipmentCape,
                    EquipmentType.Pet => GameAsset.instance.categoryEquipmentPet,
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private static string GetItemInstanceInfo(EquipmentItemInstance item)
        {
            if (item == null) return "";

            var text = $"Type: {ObjectNames.NicifyVariableName(item.GetItemBase<EquipmentItem>().type.ToString())}\n";
            
            text += $"Rarity: <color=#{ColorUtility.ToHtmlStringRGBA(item.itemColor)}>{item.rarity}</color>\n";
            if (item.durability > 50)
            {
                text += $"{item.durability}/255";
            }
            else
            {
                text += $"Durability: <color=red>{item.durability}/255</color>";
            }

            return text;
        }

        private static string GetEquipmentStats(EquipmentItemInstance item)
        {
            if (item == null) return "";

            var text = "";

            var minAttackBonus = item.bonus.Find((bonus) => bonus.attribute == Attribute.MinAttackPower && bonus.modifier is EffectModifier.Additive);
            var maxAttackBonus = item.bonus.Find((bonus) => bonus.attribute == Attribute.MinAttackPower && bonus.modifier is EffectModifier.Additive);
            if (minAttackBonus.value > 0 && maxAttackBonus.value > 0)
            {
                text += $"Attack Power: {minAttackBonus.value}~{maxAttackBonus.value}\n";
            }
            
            foreach (var bonus in item.bonus.Where(bonus => bonus.attribute is not (Attribute.MinAttackPower or Attribute.MaxAttackPower)))
            {
                var sign = bonus.value < 0 ? '-' : '+';
                var color = ColorUtility.ToHtmlStringRGBA(bonus.value < 0 ? Color.red : Color.cyan);
                if (bonus.modifier == EffectModifier.Multiplicative)
                {
                    text += $"{ObjectNames.NicifyVariableName(bonus.attribute.ToString())}: <color=#{color}>{sign}{Mathf.FloorToInt(bonus.value)*100}%</color>\n";
                }
                else
                {
                    text += $"{ObjectNames.NicifyVariableName(bonus.attribute.ToString())}: <color=#{color}>{sign}{Mathf.FloorToInt(bonus.value)}</color>\n";
                }
            }
            
            return text;
        }

        private static string GetEquipmentsRequirements(InventoryController inventory, ItemInstance item)
        {
            if (inventory == null || item == null) return "";

            var text = "";

            for (var i = 0; i < Enum.GetNames(typeof(Attribute)).Length; i++)
            {
                var attr = (Attribute)i;
                var ownerValue = inventory.abilitySystem.attributeSet[attr].GetCurrentValue();
                var equipValue = item.GetItemBase<EquipmentItem>().GetRequirements(attr);
                if(equipValue == 0) continue;
                
                if (ownerValue < equipValue)
                {
                    text += $"<color=red>Required {attr}: {equipValue} (Lacks: {equipValue - ownerValue})</color>\n";
                }
                else
                {
                    text += $"Required {attr}: {equipValue}\n";
                }
            }
            
            return text;
        }

        private static string GetItemDescription(ItemInstance item)
        {
            return item.itemBase.displayDescription;
        }
        
        private static string GetItemEffect(ItemInstance item)
        {
            return item == null ? "" : item.GetItemBase<EquipmentItem>().effects.Aggregate("", (current, effect) => current + (effect.displayDescription + "\n"));
        }
    }
}