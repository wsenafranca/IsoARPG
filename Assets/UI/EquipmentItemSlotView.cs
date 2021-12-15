using InventorySystem;
using Item;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EquipmentItemSlotView : ItemSlotView, ISlotViewHandler
    {
        [HideInInspector]
        public InventoryController inventoryController;

        [HideInInspector] 
        public EquipmentSlot thisSlot;
        
        private RawImage _slotTelegraph;

        private void Awake()
        {
            _slotTelegraph = transform.Find("SlotTelegraph").GetComponent<RawImage>();
            _slotTelegraph.gameObject.SetActive(false);
        }

        public override void OnBeginDrag(DragDropEventData eventData)
        {
            base.OnBeginDrag(eventData);
            _slotTelegraph.gameObject.SetActive(false);
        }

        public void OnDropArea(DragDropEventData eventData)
        {
            var slot = eventData.pointerDrag.GetComponent<ItemSlotView>();
            if (slot == null || slot.GetItem() == null) return;

            slot.CancelDrag();
            
            if (ReferenceEquals(slot.slotViewHandler, this)) return;

            if (slot.GetItem() is not EquipmentItemInstance item) return;

            if (!InventoryController.CheckEquipSlot(thisSlot, ((EquipmentItem)item.itemBase).type)) return;

            if (!inventoryController.IsEquipmentSlotEmpty(thisSlot) || !inventoryController.CheckRequirements(item)) return;

            if (slot.slotViewHandler.OnDraggedSlot(slot, eventData, this))
            {
                inventoryController.Equip(thisSlot, item);
            }
        }

        public bool OnDraggedSlot(ItemSlotView slot, DragDropEventData eventData, ISlotViewHandler source)
        {
            return inventoryController != null && inventoryController.UnEquip(thisSlot);
        }

        public void OnDiscardSlot(ItemSlotView slot, DragDropEventData eventData)
        {
            CancelDrag();
            
            if (inventoryController == null) return;
            
            var item = GetItem();
            if (!GroundController.instance.GetGroundPosition(eventData.position, out var worldPosition)) return;
                
            var itemDrop = Instantiate(item.itemBase.itemSlotPrefab);
            itemDrop.GetComponent<Collectible>().SetAsDrop(item.itemBase, item, worldPosition);
            inventoryController.UnEquip(thisSlot);
        }

        public InventoryController GetInventoryController()
        {
            return inventoryController;
        }

        public void OnDropAreaEnter(DragDropEventData eventData)
        {
            
        }

        public void OnDropAreaMove(DragDropEventData eventData)
        {
            var slot = eventData.pointerDrag.GetComponent<ItemSlotView>();
            
            if (slot == null || slot.GetItem() == null) return;
            
            if (slot.GetItem() is not EquipmentItemInstance item) return;

            Color color;
            if (InventoryController.CheckEquipSlot(thisSlot, ((EquipmentItem)item.itemBase).type) && inventoryController.CheckRequirements(item))
            {
                color = Color.green;
            }
            else
            {
                color = Color.red;
            }
            color.a = 0.25f;
            _slotTelegraph.color = color;
            _slotTelegraph.gameObject.SetActive(true);
        }
        
        public void OnDropAreaExit(DragDropEventData eventData)
        {
            _slotTelegraph.gameObject.SetActive(false);
        }
    }
}