using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DropArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            DragAndDropManager.instance.EnterDropArea(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DragAndDropManager.instance.ExitDropArea(this);
        }
    }
}