using System.Collections.Generic;
using System.Linq;
using TargetSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Player
{
    public class InputController : MonoBehaviour
    {
        private bool _isPressingLeft;
        private bool _isPressingRight;
        private Targetable _target;
        private float _lastClickTime;
        private Vector2 _lastMousePosition;
        private readonly List<RaycastResult> _results = new();
        
        public Targetable currentTarget
        {
            get => _target;
            set
            {
                if (_target == value) return;
                
                if (value == null) pointerExitTarget?.Invoke(_target);

                _target = value;
                if(_target != null) pointerEnterTarget?.Invoke(_target);
            }
        }
        
        public UnityEvent<Vector3> pointerClickGround;
        public UnityEvent<Targetable> pointerEnterTarget;
        public UnityEvent<Targetable> pointerExitTarget;
        public UnityEvent<Targetable, int> pointerClickTarget;

        private void Update()
        {
            if (DragAndDropManager.instance.dragging) return;

            _isPressingLeft = Input.GetMouseButton(0);
            _isPressingRight = Input.GetMouseButton(1);
            
            if (Time.time - _lastClickTime < 0.5f) return;

            if (currentTarget && _isPressingLeft || _isPressingRight)
            {
                pointerClickTarget?.Invoke(currentTarget, _isPressingRight ? 1 : 0);
                _lastClickTime = Time.time;
            }
            else if (_isPressingLeft && GetGroundPosition(Input.mousePosition, out var worldPosition))
            {
                pointerClickGround?.Invoke(worldPosition);
            }
        }
        
        private bool GetGroundPosition(Vector2 screenPosition, out Vector3 worldPosition)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            _results.Clear();
            EventSystem.current.RaycastAll(eventData, _results);
            
            worldPosition = Vector3.zero;

            if (_results.Count == 0) return false;

            if (_results.Any(result => result.gameObject.layer != GameAsset.instance.groundLayer.index))
            {
                _results.Clear();
                return false;
            }

            worldPosition = _results.First().worldPosition;
            _results.Clear();
            
            return true;
        }
    }
}