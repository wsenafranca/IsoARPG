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
        public static InputController instance { get; private set; }

        private bool _isPressing;
        private readonly List<RaycastResult> _results = new();
        private Targetable _target;
        private float _lastClickTime;
        
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
        public UnityEvent<Targetable> pointerClickTarget;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (DragAndDropManager.instance.dragging) return;
        
            if (Input.GetMouseButton(0))
            {
                _isPressing = true;
            }
            else if (_isPressing)
            {
                _isPressing = false;
            }
            
            if (!_isPressing || Time.time - _lastClickTime < 0.5f) return;

            if (currentTarget)
            {
                PlayerController.current.MoveToTarget(currentTarget);
                pointerClickTarget?.Invoke(currentTarget);
                _lastClickTime = Time.time;
            }
            else if (GetGroundPosition(Input.mousePosition, out var destination))
            {
                PlayerController.current.MoveToHit(destination);
                pointerClickGround?.Invoke(destination);
            }
        }
        
        public bool GetGroundPosition(Vector2 mousePosition, out Vector3 worldPosition)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };
            EventSystem.current.RaycastAll(eventData, _results);
            
            worldPosition = Vector3.zero;

            if (_results.Count == 0) return false;

            if (_results.Any(result => result.gameObject.layer != GameAsset.instance.groundLayer.index && result.gameObject != PlayerController.current.gameObject))
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