using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycaster : GraphicRaycaster
{
    public int orderPriority;
    public override int sortOrderPriority => orderPriority;
    public override int renderOrderPriority => orderPriority;
}