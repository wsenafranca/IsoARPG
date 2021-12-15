using Item;
using UnityEngine;

namespace UI
{
    public class ItemMeshView : MonoBehaviour
    {
        private Transform _itemMesh;
        private float _yaw;
        private bool _isRotating;

        private void Update()
        {
            if (!_isRotating || _itemMesh == null) return;
            
            _yaw += Time.deltaTime * 100.0f;
            _itemMesh.localRotation = Quaternion.Euler(0.0f, _yaw, 0.0f);
        }

        public void Setup(ItemInstance item, float tileWidth, float tileHeight)
        {
            if (item == null) return;
            
            var thisTransform = transform;

            if (item.itemBase.itemSlotPrefab)
            {
                _itemMesh = Instantiate(item.itemBase.itemSlotPrefab, thisTransform).transform;
                _itemMesh.GetComponent<Collectible>().SetAsItemSlot(item);
            }
            
            thisTransform.localPosition = new Vector3(
                tileWidth * 0.5f * item.itemBase.width, 
                -tileHeight * 0.5f * item.itemBase.height, 
                thisTransform.localPosition.z);
        }

        public void Clear()
        {
            if (_itemMesh == null) return;
            
            Destroy(_itemMesh.gameObject);
            _itemMesh = null;
        }

        public void StartRotateMesh()
        {
            if (_itemMesh == null) return;
            
            _isRotating = true;
        }

        public void StopRotateMesh()
        {
            _isRotating = false;
            _yaw = 0.0f;
            
            if (_itemMesh == null) return;
            _itemMesh.localRotation = Quaternion.identity;
        }
    }
}