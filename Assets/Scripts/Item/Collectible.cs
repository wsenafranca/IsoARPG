using InventorySystem;
using TargetSystem;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Item
{
    public class Collectible : Targetable
    {
        public ItemBase item;
        public float duration = 600.0f;

        private ItemInstance _itemInstance;

        public Collectible()
        {
            targetType = TargetType.Collectible;
        }

        public void Collect(GameObject collector)
        {
            ItemTooltipView.instance.HideTooltip();
        
            if (!item || !collector) return;
            
            var inventory = collector.GetComponent<InventoryController>();
            if (inventory && inventory.EquipOrAddItem(_itemInstance))
            {
                Destroy(gameObject);
            }
        }

        public void SetAsDrop(ItemBase itemBase, ItemInstance itemInstance, Vector3 position)
        {
            item = itemBase;
            _itemInstance = itemInstance;

            if (item == null) return;
            
            _itemInstance ??= item.CreateItemInstance();
            
            if (_itemInstance == null) return;
            
            if (duration > 0)
            {
                Destroy(gameObject, duration);
            }

            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = ShadowCastingMode.On;
                r.gameObject.layer = GameAsset.instance.outlineLayerOff.index;
            }
            
            GetComponent<Collider>().enabled = true;

            position.y += 0.1f;
            transform.SetPositionAndRotation(position, Quaternion.Euler(-90, 0.0f, 0.0f));
        }
        
        public void SetAsItemSlot(ItemInstance itemInstance)
        {
            _itemInstance = itemInstance;
            item = itemInstance.itemBase;
            
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.gameObject.layer = GameAsset.instance.uiLayer.index;
            }
            
            GetComponent<Collider>().enabled = false;
        }

        protected override Color GetTargetColor()
        {
            return _itemInstance.itemColor;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            ItemTooltipView.instance.ShowItem(_itemInstance, eventData.position);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            ItemTooltipView.instance.HideTooltip();
        }
    }
}