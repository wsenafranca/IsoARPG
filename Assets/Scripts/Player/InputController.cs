using TargetSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class InputController : MonoBehaviour
    {
        private bool _isPressingLeft;
        private bool _isPressingRight;
        private Targetable _target;
        private float _lastClickTime;
        private Vector2 _lastMousePosition;
        
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
            else if (_isPressingLeft && WorldRaycaster.GetGroundPosition(Input.mousePosition, out var worldPosition))
            {
                pointerClickGround?.Invoke(worldPosition);
            }
        }
    }
}