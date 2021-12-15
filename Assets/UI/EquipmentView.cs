using System;
using InventorySystem;
using Item;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class EquipmentView : MonoBehaviour, IDropHandler
    {
        public InventoryController inventoryController;
    
        private EquipmentItemSlotView[] _equipmentSlots;
        
        private void Awake()
        {
            _equipmentSlots = new EquipmentItemSlotView[Enum.GetNames(typeof(EquipmentSlot)).Length];
            _equipmentSlots[(int)EquipmentSlot.Helm] = transform.Find("HeadSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Armor] = transform.Find("ArmorSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Pants] = transform.Find("PantsSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Gloves] = transform.Find("GlovesSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Boots] = transform.Find("BootsSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.MainHand] = transform.Find("MainHandSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.OffHand] = transform.Find("OffHandSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Ring1] = transform.Find("Ring1Slot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Ring2] = transform.Find("Ring2Slot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Necklace] = transform.Find("NecklaceSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Cape] = transform.Find("CapeSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();
            _equipmentSlots[(int)EquipmentSlot.Pet] = transform.Find("PetSlot").Find("Slot").GetComponent<EquipmentItemSlotView>();

            for (var i = 0; i < _equipmentSlots.Length; i++)
            {
                _equipmentSlots[i].thisSlot = (EquipmentSlot)i;
                _equipmentSlots[i].inventoryController = inventoryController;
                _equipmentSlots[i].SetItem(null, 0, 0, _equipmentSlots[i]);
            }
        }

        public void OnEquip(EquipmentItemInstance item, EquipmentSlot slot)
        {
            var equipmentSlot = _equipmentSlots[(int)slot];
            equipmentSlot.SetItem(item, 0, 0, equipmentSlot);
        }
        
        public void OnUnEquip(EquipmentItemInstance item, EquipmentSlot slot)
        {
            var equipmentSlot = _equipmentSlots[(int)slot];
            equipmentSlot.RemoveItem();
            equipmentSlot.SetBackground(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var slot = eventData.pointerDrag.GetComponent<ItemSlotView>();
            if (slot == null || slot.GetItem() == null) return;
            
            slot.CancelDrag();
        }
    }
}