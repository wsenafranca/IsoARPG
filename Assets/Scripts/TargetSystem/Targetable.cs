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
    
    public class Targetable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TargetType targetType;
        
        private Renderer[] _renderers;

        protected virtual void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer or SkinnedMeshRenderer).ToArray();
        }

        public virtual Color targetColor => targetType switch
        {
            TargetType.Neutral => GameAsset.instance.outlineNeutral,
            TargetType.Enemy => GameAsset.instance.outlineEnemy,
            TargetType.Talkative => GameAsset.instance.outlineInteractable,
            TargetType.Collectible => Color.white,
            _ => throw new ArgumentOutOfRangeException()
        };

        public virtual bool isValid => true;

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!isValid)
            {
                OnPointerExit(eventData);
                return;
            }
            
            OutlineSettings.currentColor = targetColor;
            
            foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
            {
                r.gameObject.layer = GameAsset.instance.outlineLayerOn.index;
            }

            PlayerController.instance.input.currentTarget = this;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
            {
                r.gameObject.layer = GameAsset.instance.outlineLayerOff.index;
            }
        
            PlayerController.instance.input.currentTarget = null;
        }
    }
}