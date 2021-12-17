using System;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityFx.Outline;

namespace TargetSystem
{
    public enum TargetType
    {
        Neutral,
        Enemy,
        Talkative,
        Collectible
    }
    
    public class Targetable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public TargetType targetType;
        
        private Renderer[] _renderers;

        protected virtual void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer or SkinnedMeshRenderer).ToArray();
        }

        protected virtual void OnDisable()
        {
            if (PlayerController.instance.currentTarget == this)
            {
                PlayerController.instance.currentTarget = null;
            }
        }

        protected virtual Color GetTargetColor()
        {
            return targetType switch
            {
                TargetType.Neutral => GameAsset.instance.outlineNeutral,
                TargetType.Enemy => GameAsset.instance.outlineEnemy,
                TargetType.Talkative => GameAsset.instance.outlineInteractable,
                TargetType.Collectible => Color.white,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            switch (targetType)
            {
                case TargetType.Neutral:
                    Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
                    break;
                case TargetType.Enemy:
                    Cursor.SetCursor(GameAsset.instance.cursorAttack, Vector2.zero, CursorMode.Auto);
                    break;
                case TargetType.Talkative:
                    Cursor.SetCursor(GameAsset.instance.cursorTalk, Vector2.zero, CursorMode.Auto);
                    break;
                case TargetType.Collectible:
                    Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OutlineSettings.currentColor = GetTargetColor();
            
            foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
            {
                r.gameObject.layer = GameAsset.instance.outlineLayerOn.index;
            }

            PlayerController.instance.currentTarget = this;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
            
            foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
            {
                r.gameObject.layer = GameAsset.instance.outlineLayerOff.index;
            }
        
            if (PlayerController.instance.currentTarget == this)
            {
                PlayerController.instance.currentTarget = null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PlayerController.instance.MoveToTarget(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            
        }
    }
}