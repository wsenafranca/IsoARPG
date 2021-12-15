using System.Collections.Generic;
using Controller;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class GroundController : MonoBehaviour
{
    public static GroundController instance { get; private set; } 
        
    public LayerMask groundLayer;
        
    private bool _isPressing;
    private readonly List<RaycastResult> _results = new();

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
            
        if (!_isPressing) return;

        if (!GetGroundPosition(Input.mousePosition, out var destination)) return;
            
        PlayerController.instance.MoveToHit(destination);
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

        var raycastResult = new RaycastResult();
        var foundNotGround = false;
        foreach (var result in _results)
        {
            if (((1 << result.gameObject.layer) & groundLayer) == 0)
            {
                foundNotGround = true;
                break;
            }
            raycastResult = result;
        }

        _results.Clear();
        
        if (foundNotGround) return false;

        worldPosition = raycastResult.worldPosition;
        return true;
    }
}