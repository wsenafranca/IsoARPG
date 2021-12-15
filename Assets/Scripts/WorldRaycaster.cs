using UnityEngine;
using UnityEngine.EventSystems;

public class WorldRaycaster : PhysicsRaycaster
{
    public int orderPriority;
    public override int sortOrderPriority => orderPriority;
    public override int renderOrderPriority => orderPriority;
}