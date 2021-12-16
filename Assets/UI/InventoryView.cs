using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using Item;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class InventoryView : MonoBehaviour, ISlotViewHandler
    {
        public InventoryController inventoryController;
        
        private RectTransform _rect;
        private RectTransform _slotTemplate;
        private RectTransform _slotTelegraph;
        private readonly List<ItemSlotView> _slots = new();

        private int _rows;
        private int _columns;

        private float tileWidth => _slotTemplate.rect.width;
        private float tileHeight => _slotTemplate.rect.height;

        private void Awake()
        {
            if (!inventoryController) return;
            
            _rect = GetComponent<RectTransform>();
            
            _slotTemplate = transform.Find("ItemSlot").GetComponent<RectTransform>();
            _slotTemplate.gameObject.SetActive(false);

            _slotTelegraph = transform.Find("SlotTelegraph").GetComponent<RectTransform>();
            _slotTelegraph.gameObject.SetActive(false);

            var gridSize = _rect.rect.size;
            var slotSize = _slotTemplate.rect.size;

            _rows = (int)(gridSize.y / slotSize.y);
            _columns = (int)(gridSize.x / slotSize.x);
            
            inventoryController.SetSize(_rows, _columns);
            
            _slots.Clear();
        }

        public void OnAddItem(IInventoryItem item, int x, int y)
        {
            if (item == null || _slots == null) return;
            
            var slot = Instantiate(_slotTemplate, transform).GetComponent<ItemSlotView>();
            SetSlotItem(slot, (ItemInstance)item, x, y);
            slot.gameObject.SetActive(true);
            _slots.Add(slot);
        }
        
        public void OnRemoveItem(IInventoryItem item, int x, int y)
        {
            foreach (var slot in _slots.Where(slot => slot != null && slot.GetItem() == item))
            {
                Destroy(slot.gameObject);
            }
        }
        
        public bool OnDraggedSlot(ItemSlotView slot, DragDropEventData eventData, ISlotViewHandler source)
        {
            if (slot == null || slot.GetItem() == null) return false;
            
            return inventoryController.RemoveItem(slot.GetItem());
        }

        public void OnDiscardSlot(ItemSlotView slot, DragDropEventData eventData)
        {
            if (slot == null || slot.GetItem() == null) return;
            
            slot.CancelDrag();
            
            var item = slot.GetItem();
            if (!GroundController.instance.GetGroundPosition(eventData.position, out var worldPosition)) return;
                
            var itemDrop = Instantiate(item.itemBase.itemSlotPrefab);
            itemDrop.GetComponent<Collectible>().SetAsDrop(item.itemBase, item, worldPosition);
            inventoryController.RemoveItem(item);
        }

        public InventoryController GetInventoryController()
        {
            return inventoryController;
        }

        public void OnDropArea(DragDropEventData eventData)
        {
            _slotTelegraph.gameObject.SetActive(false);
            
            var slot = eventData.pointerDrag.GetComponent<ItemSlotView>();
            if (slot == null || slot.GetItem() == null) return;

            var screePoint = RectTransformUtility.WorldToScreenPoint(eventData.eventCamera, slot.rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, screePoint, eventData.eventCamera, out var slotPosition);
            slotPosition += new Vector2(_rect.rect.width * (1 - _rect.pivot.x), _rect.rect.height * (_rect.pivot.y - 1));
            
            var item = slot.GetItem();
            var tileX = (int)(slotPosition.x / tileWidth);
            var tileY = (int)(-slotPosition.y / tileHeight);
            var itemWidth = item.itemBase.width;
            var itemHeight = item.itemBase.height;
            
            if (!ReferenceEquals(slot.slotViewHandler, this))
            {
                if (inventoryController.IsEmpty(tileX, tileY, itemWidth, itemHeight) && slot.slotViewHandler.OnDraggedSlot(slot, eventData, this))
                {
                    inventoryController.AddItem(item, tileX, tileY);
                }
                slot.CancelDrag();
            }
            else if (inventoryController.MoveItem(item, tileX, tileY))
            {
                SetSlotPosition(slot, tileX, tileY);
                slot.SetBackground(true);
                slot.SendToBack();
            }
            else
            {
                slot.CancelDrag();
            }
        }
        
        private void SetSlotItem(ItemSlotView slot, ItemInstance item, int tileX, int tileY)
        {
            slot.SetItem(item, tileWidth, tileHeight, this);
            
            slot.rect.sizeDelta = new Vector2(item.itemBase.width * tileWidth, item.itemBase.height * tileHeight);
            slot.rect.anchoredPosition = new Vector2(tileX * tileWidth, -tileY * tileHeight);
        }

        private void SetSlotPosition(ItemSlotView slot, int tileX, int tileY)
        {
            slot.rect.anchoredPosition = new Vector2(tileX * tileWidth, -tileY * tileHeight);
        }

        public void OnDropAreaEnter(DragDropEventData eventData)
        {
            _slotTelegraph.SetAsLastSibling();
        }

        public void OnDropAreaMove(DragDropEventData eventData)
        {
            var slot = eventData.pointerDrag.GetComponent<ItemSlotView>();
            
            if (slot == null || slot.GetItem() == null) return;

            var screePoint = RectTransformUtility.WorldToScreenPoint(eventData.eventCamera, slot.rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, screePoint, eventData.eventCamera, out var slotPosition);
            slotPosition += new Vector2(_rect.rect.width * (1 - _rect.pivot.x), _rect.rect.height * (_rect.pivot.y - 1));
            
            var tileX = (int)(slotPosition.x / tileWidth);
            var tileY = (int)(-slotPosition.y / tileHeight);
            var itemWidth = slot.GetItem().itemBase.width;
            var itemHeight = slot.GetItem().itemBase.height;

            if (!inventoryController.IsValidRect(tileX, tileY, itemWidth, itemHeight))
            {
                _slotTelegraph.gameObject.SetActive(false);
                return;
            }
            
            var color = inventoryController.IsEmpty(tileX, tileY, slot.GetItem()) ? Color.green : Color.red;
            color.a = 0.25f;

            _slotTelegraph.GetComponent<RawImage>().color = color;
            _slotTelegraph.anchoredPosition = new Vector2(tileX * tileWidth, -tileY * tileHeight);
            _slotTelegraph.sizeDelta = new Vector2(itemWidth * tileWidth, itemHeight * tileHeight);
            _slotTelegraph.gameObject.SetActive(true);
        }

        public void OnDropAreaExit(DragDropEventData eventData)
        {
            _slotTelegraph.gameObject.SetActive(false);
        }
    }
}
