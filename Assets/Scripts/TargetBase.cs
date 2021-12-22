using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityFx.Outline;

public abstract class TargetBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Renderer[] _renderers;
        
    public abstract Color outlineColor { get; }
        
    public abstract bool isTargetValid { get; }

    protected virtual void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer or SkinnedMeshRenderer).ToArray();
    }

    protected virtual void OnDisable()
    {
        foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
        {
            r.gameObject.layer = GameAsset.instance.outlineLayerOff.index;
        }
        
        PlayerController.instance.input.RemoveTarget(this);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        OutlineSettings.currentColor = outlineColor;
            
        foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
        {
            r.gameObject.layer = GameAsset.instance.outlineLayerOn.index;
        }

        PlayerController.instance.input.AddTarget(this);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        foreach (var r in _renderers.Where(r => r.enabled && r.gameObject.activeInHierarchy))
        {
            r.gameObject.layer = GameAsset.instance.outlineLayerOff.index;
        }

        PlayerController.instance.input.RemoveTarget(this);
    }
}