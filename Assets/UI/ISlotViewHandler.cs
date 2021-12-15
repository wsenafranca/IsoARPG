using InventorySystem;
using UnityEngine.EventSystems;

namespace UI
{
    public interface ISlotViewHandler : IDropAreaHandler
    {
        public bool OnDraggedSlot(ItemSlotView slot, DragDropEventData eventData, ISlotViewHandler source);
        public void OnDiscardSlot(ItemSlotView slot, DragDropEventData eventData);

        public InventoryController GetInventoryController();
    }
}