using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InventorySystem
{
    public class InventoryController : MonoBehaviour
    {
        public int rows { get; private set; }
        public int columns { get; private set; }
        
        [Header("Sockets")]
        public EquipmentSlotSocket sockets;
        
        [Header("Visual")]
        [SerializeField] 
        private Transform rootBone;
        
        [SerializeField]
        private EquipmentVisualData[] defaultParts;
        
        private IInventoryEquipmentItem[] _slots;
        private InventoryEquipmentParts _equipmentParts;

        private InventoryContainer _items;

        [Header("Events")]
        
        public UnityEvent<IInventoryItem, int, int> onAddItem;
        
        public UnityEvent<IInventoryItem, int, int> onRemoveItem;
        
        public UnityEvent<IInventoryEquipmentItem, EquipmentSlot> onEquip;
        
        public UnityEvent<IInventoryEquipmentItem, EquipmentSlot> onUnEquip;

        public bool IsValidRect(int x, int y, int width, int height) => _items.IsValidRect(x, y, width, height);

        public bool IsEmpty(int x, int y, int width, int height) => _items.IsEmpty(x, y, width, height);
        
        public bool IsEmpty(int x, int y, IInventoryItem item) => _items.IsEmpty(x, y, item);

        public void SetSize(int inventoryRows, int inventoryColumns)
        {
            rows = inventoryRows;
            columns = inventoryColumns;
            
            _items = new InventoryContainer(this.rows, this.columns);
        }
        
        protected virtual void Awake()
        {
            var len = Enum.GetNames(typeof(EquipmentSlot)).Length;
            _slots = new IInventoryEquipmentItem[len];
            _equipmentParts = new InventoryEquipmentParts(rootBone);

            foreach (var part in defaultParts)
            {
                _equipmentParts.ReplaceParts(part);
            }
        }

        public bool AddItem(IInventoryItem item)
        {
            if (_items == null || !_items.AddItem(item, out var x, out var y)) return false;

            onAddItem?.Invoke(item, x, y);
            return true;
        }

        public bool EquipOrAddItem(IInventoryItem item)
        {
            if (TryGetEmptySlot(item, out var slot) && IsEquipmentSlotEmpty(slot) && Equip(slot, item as IInventoryEquipmentItem))
            {
                return true;
            }

            return AddItem(item);
        }
        
        public virtual bool AddItem(IInventoryItem item, int x, int y)
        {
            if (_items == null || !_items.AddItem(item, x, y)) return false;

            onAddItem?.Invoke(item, x, y);
            return true;
        }

        public virtual bool RemoveItem(IInventoryItem item)
        {
            if (_items == null || !_items.RemoveItem(item, out var x, out var y)) return false;

            onRemoveItem?.Invoke(item, x, y);
            return true;
        }

        public bool MoveItem(IInventoryItem item, int x, int y)
        {
            return _items != null && _items.MoveItem(item, x, y);
        }

        public virtual bool Equip(EquipmentSlot slot, IInventoryEquipmentItem item)
        {
            if (!CheckEquipSlot(slot, item.equipmentType)) return false;

            if (!IsEquipmentSlotEmpty(slot)) return false;

            if (!CheckRequirements(item)) return false;

            var slotIndex = (int)slot;

            _slots[slotIndex] = item;

            foreach (var part in item.visualItemPrefab)
            {
                _equipmentParts.ReplaceParts(part);
            }
            
            _equipmentParts.AttachParts(slot, item.attachItemPrefab, GetSlotSocket(slot, item.equipmentType));

            onEquip?.Invoke(item, slot);

            return true;
        }

        public virtual bool UnEquip(EquipmentSlot slot, out IInventoryEquipmentItem item)
        {
            item = null;
            var slotIndex = (int)slot;
            if (_slots[slotIndex] == null) return false;

            item = _slots[slotIndex];
            _slots[slotIndex] = null;
            
            foreach (var entry in item.visualItemPrefab)
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

        public bool CheckRequirements(IInventoryEquipmentItem item)
        {
            var owner = GetComponent<IInventoryRequirementsSource>();
            return owner != null && item != null && item.requirements.All(req => owner.GetRequirementsValue(req.requirement) >= req.value);
        }

        public bool TryGetEmptySlot(IInventoryItem item, out EquipmentSlot slot)
        {
            slot = 0;
            return item is IInventoryEquipmentItem equipmentItem && TryGetEmptySlot(equipmentItem, out slot);
        }

        public bool TryGetEmptySlot(IInventoryEquipmentItem item, out EquipmentSlot slot)
        {
            slot = 0;
            return TryGetEmptySlot(item.equipmentType, out slot);
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

        public Transform GetSlotSocket(EquipmentSlot slot, EquipmentType equipmentType)
        {
            return slot switch
            {
                EquipmentSlot.MainHand => sockets.mainHand,
                EquipmentSlot.OffHand => equipmentType == EquipmentType.Shield ? sockets.shield : sockets.offHand,
                EquipmentSlot.Helm => sockets.helm,
                EquipmentSlot.Armor => sockets.armor,
                EquipmentSlot.Pants => sockets.pants,
                EquipmentSlot.Gloves => sockets.gloves,
                EquipmentSlot.Boots => sockets.boots,
                EquipmentSlot.Necklace => sockets.necklace,
                EquipmentSlot.Ring1 => sockets.ring1,
                EquipmentSlot.Ring2 => sockets.ring2,
                EquipmentSlot.Cape => sockets.cape,
                EquipmentSlot.Pet => sockets.pet,
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        public IInventoryEquipmentItem GetEquipmentItem(EquipmentSlot slot)
        {
            return _slots[(int)slot];
        }

        public bool TryGetEquipmentItem(EquipmentSlot slot, out IInventoryEquipmentItem item)
        {
            item = _slots[(int)slot];
            return item != null;
        }

        public bool TryGetEquipmentSlot(IInventoryEquipmentItem item, out EquipmentSlot slot)
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

        public bool TryGetAttachedParts(EquipmentSlot slot, out GameObject obj) => _equipmentParts.TryGetAttachedParts(slot, out obj);

        public string DumpData()
        {
            return _items.ToString();
        }
    }
}