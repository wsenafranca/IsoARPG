using System;
using AbilitySystem;
using AttributeSystem;
using Item;
using UnityEngine;
using UnityEngine.Events;

namespace InventorySystem
{
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class InventoryController : MonoBehaviour, IWeaponMeleeControllerInterface
    {
        public AbilitySystemComponent abilitySystem;
        public int rows { get; private set; }
        public int columns { get; private set; }
        
        [Header("Sockets")]
        public EquipmentSlotSocket sockets;
        
        [Header("Visual")]
        [SerializeField] 
        private Transform rootBone;
        
        [SerializeField]
        private EquipmentVisualData[] defaultParts;
        
        private EquipmentItemInstance[] _slots;
        private InventoryEquipmentParts _equipmentParts;

        private InventoryContainer _items;

        [Header("Events")]
        
        public UnityEvent<ItemInstance, int, int> onAddItem;
        
        public UnityEvent<ItemInstance, int, int> onRemoveItem;
        
        public UnityEvent<EquipmentItemInstance, EquipmentSlot> onEquip;
        
        public UnityEvent<EquipmentItemInstance, EquipmentSlot> onUnEquip;

        public bool IsValidRect(int x, int y, int width, int height) => _items.IsValidRect(x, y, width, height);

        public bool IsEmpty(int x, int y, int width, int height) => _items.IsEmpty(x, y, width, height);
        
        public bool IsEmpty(int x, int y, ItemInstance item) => _items.IsEmpty(x, y, item);

        public void SetSize(int inventoryRows, int inventoryColumns)
        {
            rows = inventoryRows;
            columns = inventoryColumns;
            
            _items = new InventoryContainer(this.rows, this.columns);
        }
        
        private void Awake()
        {
            abilitySystem = GetComponent<AbilitySystemComponent>();
            
            var len = Enum.GetNames(typeof(EquipmentSlot)).Length;
            _slots = new EquipmentItemInstance[len];
            _equipmentParts = new InventoryEquipmentParts(rootBone);

            foreach (var part in defaultParts)
            {
                _equipmentParts.ReplaceParts(part);
            }
        }

        public bool AddItem(ItemInstance item)
        {
            if (_items == null || !_items.AddItem(item, out var x, out var y)) return false;

            onAddItem?.Invoke(item, x, y);
            return true;
        }

        public bool EquipOrAddItem(ItemInstance item)
        {
            if (TryGetEmptySlot(item, out var slot) && IsEquipmentSlotEmpty(slot) && Equip(slot, item as EquipmentItemInstance))
            {
                return true;
            }

            return AddItem(item);
        }
        
        public bool AddItem(ItemInstance item, int x, int y)
        {
            if (_items == null || !_items.AddItem(item, x, y)) return false;

            onAddItem?.Invoke(item, x, y);
            return true;
        }

        public bool RemoveItem(ItemInstance item)
        {
            if (_items == null || !_items.RemoveItem(item, out var x, out var y)) return false;

            onRemoveItem?.Invoke(item, x, y);
            return true;
        }

        public bool MoveItem(ItemInstance item, int x, int y)
        {
            return _items != null && _items.MoveItem(item, x, y);
        }

        public bool Equip(EquipmentSlot slot, EquipmentItemInstance item)
        {
            if (item is not { itemBase: EquipmentItem equipBase }) return false;
            
            if (!CheckEquipSlot(slot, equipBase.type)) return false;

            if (!IsEquipmentSlotEmpty(slot)) return false;

            if (!CheckRequirements(item)) return false;

            var slotIndex = (int)slot;

            _slots[slotIndex] = item;

            foreach (var part in equipBase.visualItemPrefab)
            {
                _equipmentParts.ReplaceParts(part);
            }
            
            _equipmentParts.AttachParts(slot, equipBase.attachItemPrefab, GetSlotSocket(slot));

            onEquip?.Invoke(item, slot);

            return true;
        }

        public bool UnEquip(EquipmentSlot slot)
        {
            var slotIndex = (int)slot;
            if (_slots[slotIndex] == null) return false;

            var item = _slots[slotIndex];
            _slots[slotIndex] = null;

            if (item is not { itemBase: EquipmentItem equipBase }) return true;
            
            foreach (var entry in equipBase.visualItemPrefab)
            {
                _equipmentParts.ReplaceParts(defaultParts[(int)entry.slot]);
            }
            _equipmentParts.DetachParts(slot);
            
            onUnEquip?.Invoke(item, slot);

            return true;
        }
        
        public static bool CheckEquipSlot(EquipmentSlot slot, EquipmentType type)
        {
            return slot switch
            {
                EquipmentSlot.MainHand => type is EquipmentType.OneHandWeapon or EquipmentType.TwoHandWeapon,
                EquipmentSlot.OffHand => type is EquipmentType.OneHandWeapon or EquipmentType.Shield,
                EquipmentSlot.Helm => type == EquipmentType.Helm,
                EquipmentSlot.Armor => type == EquipmentType.Armor,
                EquipmentSlot.Pants => type == EquipmentType.Pants,
                EquipmentSlot.Gloves => type == EquipmentType.Gloves,
                EquipmentSlot.Boots => type == EquipmentType.Boots,
                EquipmentSlot.Necklace => type == EquipmentType.Necklace,
                EquipmentSlot.Ring1 => type == EquipmentType.Ring,
                EquipmentSlot.Ring2 => type == EquipmentType.Ring,
                EquipmentSlot.Cape => type == EquipmentType.Cape,
                EquipmentSlot.Pet => type == EquipmentType.Pet,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        public bool CheckRequirements(EquipmentItemInstance item)
        {
            var primaryAttributes = GetComponent<PrimaryAttributeSet>();
            if (!primaryAttributes) return false;

            if (primaryAttributes.currentLevel < item.GetItemBase<EquipmentItem>().GetRequirements(EquipmentRequirement.Level))
            {
                return false;
            }
            
            if (primaryAttributes.currentStrength < item.GetItemBase<EquipmentItem>().GetRequirements(EquipmentRequirement.Strength))
            {
                return false;
            }

            if (primaryAttributes.currentStamina < item.GetItemBase<EquipmentItem>().GetRequirements(EquipmentRequirement.Stamina))
            {
                return false;
            }
            
            if (primaryAttributes.currentDexterity < item.GetItemBase<EquipmentItem>().GetRequirements(EquipmentRequirement.Dexterity))
            {
                return false;
            }
            
            if (primaryAttributes.currentIntelligence < item.GetItemBase<EquipmentItem>().GetRequirements(EquipmentRequirement.Intelligence))
            {
                return false;
            }

            return true;
        }

        public bool TryGetEmptySlot(ItemInstance item, out EquipmentSlot slot)
        {
            slot = 0;
            return item is EquipmentItemInstance equipmentItem && TryGetEmptySlot(equipmentItem, out slot);
        }

        public bool TryGetEmptySlot(EquipmentItemInstance item, out EquipmentSlot slot)
        {
            slot = 0;
            return item is { itemBase: EquipmentItem equipBase } && TryGetEmptySlot(equipBase.type, out slot);
        }

        public bool TryGetEmptySlot(EquipmentType type, out EquipmentSlot slot)
        {
            switch (type)
            {
                case EquipmentType.OneHandWeapon:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.MainHand))
                    {
                        slot = EquipmentSlot.MainHand;
                        return true;
                    }
                    else if (IsEquipmentSlotEmpty(EquipmentSlot.OffHand))
                    {
                        slot = EquipmentSlot.OffHand;
                        return true;
                    }
                    break;
                case EquipmentType.TwoHandWeapon:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.MainHand) && IsEquipmentSlotEmpty(EquipmentSlot.OffHand))
                    {
                        slot = EquipmentSlot.MainHand;
                        return true;
                    }
                    break;
                case EquipmentType.Shield:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.OffHand))
                    {
                        slot = EquipmentSlot.OffHand;
                        return true;
                    }
                    break;
                case EquipmentType.Helm:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Helm))
                    {
                        slot = EquipmentSlot.Helm;
                        return true;
                    }
                    break;
                case EquipmentType.Armor:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Armor))
                    {
                        slot = EquipmentSlot.Armor;
                        return true;
                    }
                    break;
                case EquipmentType.Pants:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Pants))
                    {
                        slot = EquipmentSlot.Pants;
                        return true;
                    }
                    break;
                case EquipmentType.Gloves:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Gloves))
                    {
                        slot = EquipmentSlot.Gloves;
                        return true;
                    }
                    break;
                case EquipmentType.Boots:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Boots))
                    {
                        slot = EquipmentSlot.Boots;
                        return true;
                    }
                    break;
                case EquipmentType.Necklace:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Necklace))
                    {
                        slot = EquipmentSlot.Necklace;
                        return true;
                    }
                    break;
                case EquipmentType.Ring:
                    if (IsEquipmentSlotEmpty(EquipmentSlot.Ring1))
                    {
                        slot = EquipmentSlot.Ring1;
                        return true;
                    }
                    else if (IsEquipmentSlotEmpty(EquipmentSlot.Ring2))
                    {
                        slot = EquipmentSlot.Ring2;
                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            slot = 0;
            return false;
        }

        public bool IsEquipmentSlotEmpty(EquipmentSlot slot)
        {
            return _slots[(int)slot] == null;
        }

        public Transform GetSlotSocket(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.MainHand => sockets.mainHand,
                EquipmentSlot.OffHand => sockets.offHand,
                EquipmentSlot.Helm => sockets.helm,
                EquipmentSlot.Armor => sockets.armor,
                EquipmentSlot.Pants => sockets.pants,
                EquipmentSlot.Gloves => sockets.gloves,
                EquipmentSlot.Boots => sockets.boots,
                EquipmentSlot.Necklace => sockets.necklace,
                EquipmentSlot.Ring1 => sockets.ring1,
                EquipmentSlot.Ring2 => sockets.ring2,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        public EquipmentItemInstance GetEquipmentItem(EquipmentSlot slot)
        {
            return _slots[(int)slot];
        }

        public bool TryGetEquipmentSlot(EquipmentItemInstance item, out EquipmentSlot slot)
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                if (item != _slots[i]) continue;
                
                slot = (EquipmentSlot)i;
                return true;
            }

            slot = 0;
            return false;
        }

        public WeaponMeleeController GetWeaponMeleeController(int weaponIndex)
        {
            var equipmentSlot = weaponIndex == 1 ? EquipmentSlot.OffHand : EquipmentSlot.MainHand;

            if (!_equipmentParts.TryGetAttachedParts(equipmentSlot, out var itemObj)) return null;
            
            var item = _slots[(int)equipmentSlot];

            return item is not { itemBase: EquipmentItem { isWeapon: true } } ? null : itemObj.GetComponent<WeaponMeleeController>();
        }

        public string DumpData()
        {
            return _items.ToString();
        }
    }
}