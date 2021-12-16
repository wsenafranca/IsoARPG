using Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ItemSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDraggableHandler
    {
        public Canvas canvas;
        private RawImage _background;
        private ItemMeshView _visualItem;
        private CanvasGroup _canvasGroup;

        private Vector2 _lastAnchoredPosition;

        private ItemInstance _item;

        [HideInInspector]
        public RectTransform rect;
        
        public ItemInstance GetItem()
        {
            return _item;
        }

        public ISlotViewHandler slotViewHandler { get; private set; }

        public void SetItem(ItemInstance item, float tileWidth, float tileHeight, ISlotViewHandler source)
        {
            slotViewHandler = source;
            _item = item;
            rect = GetComponent<RectTransform>();
                
            _background = GetComponent<RawImage>();
            
            _canvasGroup = GetComponent<CanvasGroup>();

            _visualItem = GetComponentInChildren<ItemMeshView>();
            _visualItem.Setup(item, tileWidth, tileHeight);
            
            SetBackground(_item != null);
            SendToBack();
        }

        public void SetBackground(bool visible)
        {
            var color = _item?.itemColor ?? _background.color;
            color.a = visible ? 0.25f : 0.0f;
            _background.color = color;
        }

        public void BringToFront()
        {
            var visualTransform = _visualItem.transform;
            var position = visualTransform.localPosition;
            position.z = -30;
            visualTransform.localPosition = position;
        }
        
        public void BringToTop()
        {
            var visualTransform = _visualItem.transform;
            var position = visualTransform.localPosition;
            position.z = -50;
            visualTransform.localPosition = position;
            
            visualTransform.localScale = new Vector3(80, 80, 80);
        }

        public void SendToBack()
        {
            var visualTransform = _visualItem.transform;
            var position = visualTransform.localPosition;
            position.z = -5;
            visualTransform.localPosition = position;
            visualTransform.localScale = new Vector3(60, 60, 60);
        }

        public void RemoveItem()
        {
            _visualItem.Clear();
            SetItem(null, 0.0f, 0.0f, slotViewHandler);
        }

        public virtual void CancelDrag()
        {
            SendToBack();
            SetBackground(_item != null);
            _canvasGroup.blocksRaycasts = true;
            
            rect.anchoredPosition = _lastAnchoredPosition;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!DragAndDropManager.instance.dragging && eventData.button == PointerEventData.InputButton.Left)
            {
                DragAndDropManager.instance.BeginDrag(gameObject);
            }
        }

        public virtual void OnBeginDrag(DragDropEventData eventData)
        {
            BringToTop();
            SetBackground(false);
            _visualItem.StopRotateMesh();
            
            _canvasGroup.blocksRaycasts = false;
            
            ItemTooltipView.instance.HideTooltip();

            _lastAnchoredPosition = rect.anchoredPosition;
            rect.SetAsLastSibling();
        }

        public virtual void OnDrag(DragDropEventData eventData)
        {
            var anchoredPosition = rect.anchoredPosition;
            anchoredPosition += eventData.delta / canvas.scaleFactor;
            rect.anchoredPosition = anchoredPosition;
            BringToTop();
            SetBackground(false);
            rect.SetAsLastSibling();
        }
        
        public virtual void OnEndDrag(DragDropEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            SendToBack();
        }

        public void OnDrop(DragDropEventData eventData)
        {
            if(eventData.area == null) slotViewHandler.OnDiscardSlot(this, eventData);
        }

        public void OnCancelDrag(DragDropEventData eventData)
        {
            CancelDrag();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (DragAndDropManager.instance.dragging) return;
            
            BringToFront();
            _visualItem.StartRotateMesh();
            ItemTooltipView.instance.ShowItem(_item, eventData.position);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            SendToBack();
            _visualItem.StopRotateMesh();
            ItemTooltipView.instance.HideTooltip();
        }
    }
}