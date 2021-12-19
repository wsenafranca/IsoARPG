using InventorySystem;
using Item;
using Player;
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

        public override void OnEndDrag(DragDropEventData eventData)
        {
            base.OnEndDrag(eventData);
            _slotTelegraph.gameObject.SetActive(false);
        }

        public void OnDropArea(DragDropEventData eventData)
        {
            _slotTelegraph.gameObject.SetActive(false);
            
            var slot = eventData.pointerDrag.GetComponent<ItemSlotView>();
            if (slot == null || slot.GetItem() == null) return;

            slot.CancelDrag();
            
            if (ReferenceEquals(slot.slotViewHandler, this)) return;

            if (slot.GetItem() is not IInventoryEquipmentItem item) return;

            if (!InventoryController.CheckEquipSlot(thisSlot, item.equipmentType)) return;

            if (!inventoryController.IsEquipmentSlotEmpty(thisSlot) || !inventoryController.CheckRequirements(item)) return;

            if (slot.slotViewHandler.OnDraggedSlot(slot, eventData, this))
            {
                inventoryController.Equip(thisSlot, item);
            }
        }

        public bool OnDraggedSlot(ItemSlotView slot, DragDropEventData eventData, ISlotViewHandler source)
        {
            _slotTelegraph.gameObject.SetActive(false);
            return inventoryController != null && inventoryController.UnEquip(thisSlot, out _);
        }

        public override void CancelDrag()
        {
            base.CancelDrag();
            _slotTelegraph.gameObject.SetActive(false);
        }

        public void OnDiscardSlot(ItemSlotView slot, DragDropEventData eventData)
        {
            CancelDrag();
            
            if (inventoryController == null) return;
            
            var item = GetItem();
            if (!WorldRaycaster.GetGroundPosition(eventData.position, out var worldPosition, inventoryController.gameObject)) return;
                
            var itemDrop = Instantiate(item.itemBase.itemSlotPrefab);
            itemDrop.GetComponent<Collectible>().SetAsDrop(item.itemBase, item, worldPosition);
            inventoryController.UnEquip(thisSlot, out _);
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
            
            if (slot.GetItem() is not IInventoryEquipmentItem item) return;

            Color color;
            if (InventoryController.CheckEquipSlot(thisSlot, item.equipmentType) && inventoryController.CheckRequirements(item))
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