using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldRaycaster : PhysicsRaycaster
{
    public int orderPriority;
    public override int sortOrderPriority => orderPriority;
    public override int renderOrderPriority => orderPriority;
    
    public static bool GetGroundPosition(Vector2 mousePosition, out Vector3 worldPosition, GameObject ignoreObject = null)
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
            
        worldPosition = Vector3.zero;

        if (results.Count == 0) return false;

        if (results.Any(result => result.gameObject.layer != GameAsset.instance.groundLayer.index && result.gameObject != ignoreObject))
        {
            return false;
        }

        worldPosition = results.First().worldPosition;
        results.Clear();
            
        return true;
    }
}