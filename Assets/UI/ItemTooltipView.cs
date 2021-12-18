using System;
using System.Linq;
using AttributeSystem;
using InventorySystem;
using Item;
using Player;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Attribute = AttributeSystem.Attribute;

namespace UI
{
    public class ItemTooltipView : MonoBehaviour
    {
        public static ItemTooltipView instance { get; private set; }

        public Canvas canvas;
        
        private TextMeshProUGUI _itemName;
        private RawImage _itemIcon;
        private TextMeshProUGUI _itemInstanceInfo;
        private TextMeshProUGUI _equipmentStatsModifier;
        private TextMeshProUGUI _equipmentBonusModifier;
        private TextMeshProUGUI _equipmentRequirements;
        private TextMeshProUGUI _itemEffects;
        private TextMeshProUGUI _itemDescription;

        private void Awake()
        {
            _itemName = transform.Find("Header").transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            _itemIcon = transform.Find("Header").transform.Find("ItemIcon").GetComponent<RawImage>();
            _itemInstanceInfo = transform.Find("ItemInstanceInfo").GetComponent<TextMeshProUGUI>();
            _equipmentStatsModifier = transform.Find("EquipmentStatsModifier").GetComponent<TextMeshProUGUI>();
            _equipmentBonusModifier = transform.Find("EquipmentBonusModifier").GetComponent<TextMeshProUGUI>();
            _equipmentRequirements = transform.Find("EquipmentRequirements").GetComponent<TextMeshProUGUI>();
            _itemEffects = transform.Find("ItemEffects").GetComponent<TextMeshProUGUI>();
            _itemDescription = transform.Find("ItemDescription").GetComponent<TextMeshProUGUI>();
            
            gameObject.SetActive(false);
            
            instance = this;
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
        
        public void ShowItem(ItemInstance item, Vector2 screenPosition)
        {
            if (item == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _itemName.text = GetItemName(item);
            _itemIcon.texture = GetIcon(item);
            _itemInstanceInfo.text = GetItemInstanceInfo(item as EquipmentItemInstance);
            _equipmentStatsModifier.text = GetEquipmentStatsModifier(item as EquipmentItemInstance);
            _equipmentBonusModifier.text = GetEquipmentBonusModifier(item as EquipmentItemInstance);
            _equipmentRequirements.text = GetEquipmentsRequirements(item, PlayerController.current.GetComponent<PrimaryAttributeSet>());
            _itemEffects.text = "";
            _itemDescription.text = GetItemDescription(item);

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

            var cam = canvas.worldCamera;
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
                ItemCategory.Equipment => item.GetItemBase<EquipmentItem>().equipmentType switch
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

            var text = $"Type: {ObjectNames.NicifyVariableName(item.GetItemBase<EquipmentItem>().equipmentType.ToString())}\n";
            
            text += $"Rarity: <color=#{ColorUtility.ToHtmlStringRGBA(item.itemColor)}>{item.rarity}</color>\n";
            text += item.durability switch
            {
                > 50 => $"Durability: {item.durability}/255",
                > 25 => $"Durability: <color=#ffff00ff>{item.durability}</color>/255",
                _ => $"Durability: <color=red>{item.durability}</color>/255"
            };

            return text;
        }

        private static string GetEquipmentStatsModifier(EquipmentItemInstance item)
        {
            if (item == null) return "";

            var text = "";

            var minAttackAttribute = item.attributes.Find(modifier => modifier.attribute == Attribute.MinAttackPower);
            var maxAttackAttribute = item.attributes.Find(modifier => modifier.attribute == Attribute.MaxAttackPower);
            if (minAttackAttribute.value != null && maxAttackAttribute.value != null)
            {
                text += $"Attack Power: {minAttackAttribute.value.currentValue}~{maxAttackAttribute.value.currentValue}\n";
            }
            
            foreach (var attribute in item.attributes.Where(attribute => attribute.attribute is not (Attribute.MinAttackPower or Attribute.MaxAttackPower)))
            {
                if(attribute.value.currentValue == 0) continue;
                text += $"{ObjectNames.NicifyVariableName(attribute.attribute.ToString())}: {attribute.value.currentValue}\n";
            }
            
            return text;
        }
        
        private static string GetEquipmentBonusModifier(EquipmentItemInstance item)
        {
            if (item == null) return "";

            var text = "";
            
            foreach (var modifier in item.additiveModifiers)
            {
                if(modifier.value == 0) continue;
                
                var sign = modifier.value < 0 ? '-' : '+';
                var color = ColorUtility.ToHtmlStringRGBA(modifier.value < 0 ? Color.red : Color.cyan);
                
                text += $"{ObjectNames.NicifyVariableName(modifier.attribute.ToString())}: <color=#{color}>{sign}{Mathf.Abs(modifier.value)}</color>\n";
            }
            
            foreach (var modifier in item.multiplicativeModifiers)
            {
                if(modifier.value == 0) continue;
                
                var sign = modifier.value < 0 ? '-' : '+';
                var color = ColorUtility.ToHtmlStringRGBA(modifier.value < 0 ? Color.red : Color.cyan);
                
                text += $"{ObjectNames.NicifyVariableName(modifier.attribute.ToString())}: <color=#{color}>{sign}{Mathf.FloorToInt(Mathf.Abs(modifier.value) * 100.0f)}%</color>\n";
            }
            
            return text;
        }

        private static string GetEquipmentsRequirements(ItemInstance item, PrimaryAttributeSet attributeSet)
        {
            if (attributeSet == null || item == null) return "";

            var text = "";

            for (var i = 0; i < Enum.GetNames(typeof(EquipmentRequirement)).Length; i++)
            {
                var req = (EquipmentRequirement)i;
                var ownerValue = req switch
                {
                    EquipmentRequirement.Level => attributeSet.currentLevel,
                    EquipmentRequirement.Strength => attributeSet.currentStrength,
                    EquipmentRequirement.Stamina => attributeSet.currentStamina,
                    EquipmentRequirement.Dexterity => attributeSet.currentDexterity,
                    EquipmentRequirement.Intelligence => attributeSet.currentIntelligence,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                var equipValue = item.GetItemBase<EquipmentItem>().GetRequirements(req);
                if(equipValue == 0) continue;
                
                if (ownerValue < equipValue)
                {
                    text += $"<color=red>Required {req}: {equipValue} (Lacks: {equipValue - ownerValue})</color>\n";
                }
                else
                {
                    text += $"Required {req}: {equipValue}\n";
                }
            }
            
            return text;
        }

        private static string GetItemDescription(ItemInstance item)
        {
            return item.itemBase.displayDescription;
        }
    }
}