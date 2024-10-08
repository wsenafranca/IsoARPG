﻿using InventorySystem;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Item
{
    public class Collectible : TargetBase
    {
        public ItemBase item;
        public float duration = 600.0f;

        private ItemInstance _itemInstance;

        private void Start()
        {
            if (item != null && _itemInstance == null)
            {
                _itemInstance = item.CreateItemInstance();
            }
        }

        public void Collect(GameObject collector)
        {
            ItemTooltipView.instance.HideTooltip();
        
            if (_itemInstance == null || collector == null) return;
            
            var inventory = collector.GetComponent<InventoryController>();
            if (!inventory || !inventory.EquipOrAddItem(_itemInstance)) return;
            
            item = null;
            _itemInstance = null;
            enabled = false;
            Destroy(gameObject);
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
            if (!gameObject.TryGetComponent<Rigidbody>(out _))
            {
                gameObject.AddComponent<Rigidbody>();
            }
            
            var spherePoint = Random.insideUnitSphere * 0.25f;
            position.x += spherePoint.x;
            position.y += Mathf.Max(0, spherePoint.y) + 1;
            position.z += spherePoint.z;

            transform.SetPositionAndRotation(position, Quaternion.Euler(90, 0.0f, 0.0f));
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
            if (gameObject.TryGetComponent<Rigidbody>(out var rb))
            {
                Destroy(rb);
            }
        }
        
        public override Color outlineColor => _itemInstance.itemColor;

        public override bool isTargetValid => item != null && _itemInstance != null;

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