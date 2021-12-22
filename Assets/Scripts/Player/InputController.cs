using System.Collections.Generic;
using System.Linq;
using Character;
using SkillSystem;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Player
{
    public class InputController : MonoBehaviour
    {
        public Skill[] skillSlot;
        
        private bool _isPressingLeft;
        private bool _isPressingRight;
        private readonly List<TargetBase> _targets = new();
        private float _lastClickTime;
        private Vector2 _lastMousePosition;
        private readonly List<RaycastResult> _results = new();

        private TargetBase _currentTarget;

        public UnityEvent<Vector3> pointerClickGround;
        public UnityEvent<TargetBase> pointerEnterTarget;
        public UnityEvent<TargetBase> pointerExitTarget;
        public UnityEvent<TargetBase, int> pointerClickTarget;

        private void Update()
        {
            if (DragAndDropManager.instance.dragging) return;

            _isPressingLeft = Input.GetMouseButton(0);
            _isPressingRight = Input.GetMouseButton(1);
            
            if (Time.time - _lastClickTime < 0.5f) return;

            if (_currentTarget && _currentTarget.isTargetValid && (_isPressingLeft || _isPressingRight))
            {
                pointerClickTarget?.Invoke(_currentTarget, _isPressingRight ? 1 : 0);
                _lastClickTime = Time.time;
            }
            else if (_isPressingLeft && TryGetGroundPosition(Input.mousePosition, out var worldPosition))
            {
                pointerClickGround?.Invoke(worldPosition);
            }
        }

        public void AddTarget(TargetBase target)
        {
            if (!_targets.Contains(target))
            {
                _targets.Add(target);
                var character = target.GetComponent<CharacterBase>();
                if(character) character.dead.AddListener(OnTargetDead);
            }

            if (_currentTarget != null) return;
            
            _currentTarget = target;
            if(_currentTarget) pointerEnterTarget?.Invoke(_currentTarget);
        }

        public void RemoveTarget(TargetBase target)
        {
            if (target == null) return;
            
            _targets.Remove(target);
            var character = target.GetComponent<CharacterBase>();
            if(character) character.dead.RemoveListener(OnTargetDead);

            if (target != _currentTarget) return;

            if (target != null)
            {
                pointerExitTarget?.Invoke(target);
            }
            
            _currentTarget = _targets.FirstOrDefault();
            if(_currentTarget) pointerEnterTarget?.Invoke(_currentTarget);
        }

        private void OnTargetDead(CharacterBase character)
        {
            RemoveTarget(character.GetComponent<TargetBase>());
        }

        private bool TryGetGroundPosition(Vector2 screenPosition, out Vector3 worldPosition)
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