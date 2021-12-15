using System;
using System.Collections.Generic;
using Item;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InventorySystem
{
    public class InventoryEquipmentParts
    {
        private readonly Transform _rootBone;
        private readonly GameObject[] _visualSlots;
        private readonly GameObject[] _attachSlots;
        
        private readonly Dictionary<string, Transform> _boneMap = new();

        public InventoryEquipmentParts(Transform rootBone)
        {
            _visualSlots = new GameObject[Enum.GetNames(typeof(EquipmentVisualSlot)).Length];
            _attachSlots = new GameObject[Enum.GetNames(typeof(EquipmentSlot)).Length];
            _rootBone = rootBone;
            
            foreach (var bone in _rootBone.GetComponentsInChildren<Transform>())
            {
                _boneMap[bone.name] = bone;
            }
        }
        
        public bool TryGetAttachedParts(EquipmentSlot slot, out GameObject obj)
        {
            obj = _attachSlots[(int)slot];
            return obj != null;
        }

        public void AttachParts(EquipmentSlot slot, GameObject prefab, Transform socketToAttach)
        {
            DetachParts(slot);
            
            if (socketToAttach == null) return;
            
            _attachSlots[(int)slot] = Object.Instantiate(prefab, socketToAttach, false);
        }
        
        public void DetachParts(EquipmentSlot slot)
        {
            var slotIndex = (int)slot;
            if (_attachSlots[slotIndex] == null) return;
            
            Object.Destroy(_attachSlots[slotIndex]);
            _attachSlots[slotIndex] = null;
        }
        
        public bool TryGetParts(EquipmentVisualSlot slot, out GameObject obj)
        {
            obj = _visualSlots[(int)slot];
            return obj != null;
        }
        
        public void ReplaceParts(EquipmentVisualData data)
        {
            RemoveParts(data);
            
            if (data.prefab == null) return;

            var prefabRenderer = data.prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            if (prefabRenderer == null) return;

            var partObject = Object.Instantiate(data.prefab, _rootBone.parent);

            var bones = new Transform[prefabRenderer.bones.Length];
            for (var j = 0; j < bones.Length; j++)
            {
                if (_boneMap.ContainsKey(prefabRenderer.bones[j].name))
                {
                    bones[j] = _boneMap[prefabRenderer.bones[j].name];
                }
                else
                {
                    Debug.Log("Not found " + prefabRenderer.bones[j].name);
                }

                var partRenderer = partObject.GetComponentInChildren<SkinnedMeshRenderer>();
                partRenderer.bones = bones;
                partRenderer.rootBone = _rootBone;

                _visualSlots[(int)data.slot] = partObject;
            }
        }
        
        public void RemoveParts(EquipmentVisualData data)
        {
            var slotIndex = (int)data.slot;
            if (_visualSlots[slotIndex] == null) return;
            
            Object.Destroy(_visualSlots[slotIndex]);
            _visualSlots[slotIndex] = null;
        }
    }
}