using System.Collections.Generic;
using System.Linq;
using Character;
using SkillSystem;
using TargetSystem;
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
        private readonly List<Targetable> _targets = new();
        private float _lastClickTime;
        private Vector2 _lastMousePosition;
        private readonly List<RaycastResult> _results = new();

        public Targetable currentTarget { get; private set; }

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

            if (currentTarget && currentTarget.enabled && (_isPressingLeft || _isPressingRight))
            {
                pointerClickTarget?.Invoke(currentTarget, _isPressingRight ? 1 : 0);
                _lastClickTime = Time.time;
            }
            else if (_isPressingLeft && TryGetGroundPosition(Input.mousePosition, out var worldPosition))
            {
                pointerClickGround?.Invoke(worldPosition);
            }
        }

        public void AddTarget(Targetable target)
        {
            if (!_targets.Contains(target))
            {
                _targets.Add(target);
                var character = target.GetComponent<CharacterBase>();
                if(character) character.dead.AddListener(OnTargetDead);
            }

            if (currentTarget != null) return;
            
            currentTarget = target;
            if(currentTarget) pointerEnterTarget?.Invoke(currentTarget);
        }

        public void RemoveTarget(Targetable target)
        {
            if (target == null) return;
            
            _targets.Remove(target);
            var character = target.GetComponent<CharacterBase>();
            if(character) character.dead.RemoveListener(OnTargetDead);

            if (target != currentTarget) return;

            if (target != null)
            {
                pointerExitTarget?.Invoke(target);
            }
            
            currentTarget = _targets.FirstOrDefault();
            if(currentTarget) pointerEnterTarget?.Invoke(currentTarget);
        }

        private void OnTargetDead(CharacterBase character)
        {
            RemoveTarget(character.GetComponent<Targetable>());
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