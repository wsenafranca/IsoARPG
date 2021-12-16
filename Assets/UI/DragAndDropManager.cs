using System.Collections;
using UnityEngine;

namespace UI
{
    public struct DragDropEventData
    {
        public GameObject pointerDrag;
        public bool dragging;
        public Camera eventCamera;
        public Vector2 position;
        public Vector2 delta;
        public DropArea area;
    }

    public interface IDropAreaHandler
    {
        public void OnDropAreaEnter(DragDropEventData eventData);
        public void OnDropAreaMove(DragDropEventData eventData);
        public void OnDropArea(DragDropEventData eventData);
        public void OnDropAreaExit(DragDropEventData eventData);
    }

    public interface IDraggableHandler
    {
        public void OnBeginDrag(DragDropEventData eventData);
        public void OnDrag(DragDropEventData eventData);
        public void OnEndDrag(DragDropEventData eventData);
        public void OnDrop(DragDropEventData eventData);
        public void OnCancelDrag(DragDropEventData eventData);
    }
    
    public class DragAndDropManager : MonoBehaviour
    {
        public static DragAndDropManager instance { get; private set; }

        public GameObject pointerDrag { get; private set; }
        
        public DropArea currentArea { get; private set; }

        public bool dragging { get; private set; }

        private Vector2 _lastMousePosition;

        public void BeginDrag(GameObject target)
        {
            if (dragging || pointerDrag != null) return;
            pointerDrag = target;

            var eventData = CreateEvent();
            
            _lastMousePosition = eventData.position;
            
            if(pointerDrag != null) pointerDrag.GetComponent<IDraggableHandler>()?.OnBeginDrag(eventData);
            
            dragging = true;
        }
        
        public void EnterDropArea(DropArea area)
        {
            if (!dragging || pointerDrag == null) return;

            currentArea = area;
            if (area == null) return;
            
            area.GetComponent<IDropAreaHandler>()?.OnDropAreaEnter(CreateEvent());
        }

        public void ExitDropArea(DropArea area)
        {
            if (!dragging || pointerDrag == null) return;

            if(currentArea == area) currentArea = null;
            if (area == null) return;
            
            area.GetComponent<IDropAreaHandler>()?.OnDropAreaExit(CreateEvent());
        }

        private void EndDrag(DragDropEventData eventData)
        {
            if (!dragging) return;

            if (pointerDrag != null)
            {
                pointerDrag.GetComponent<IDraggableHandler>()?.OnEndDrag(eventData);
            }

            if (currentArea) currentArea.GetComponent<IDropAreaHandler>()?.OnDropAreaExit(eventData);

            pointerDrag = null;
            currentArea = null;

            StartCoroutine(EndDrag_());
        }
        
        private IEnumerator EndDrag_()
        {
            yield return new WaitForSeconds(0.2f);
            dragging = false;
        }

        private void Drop(DragDropEventData eventData)
        {
            if (!dragging || pointerDrag == null) return;
            
            if (currentArea != null)
            {
                currentArea.GetComponent<IDropAreaHandler>().OnDropArea(eventData);
            }
            
            pointerDrag.GetComponent<IDraggableHandler>()?.OnDrop(eventData);
            
            EndDrag(eventData);
        }
        
        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (!dragging || pointerDrag == null) return;

            var eventData = CreateEvent();
            
            _lastMousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                Drop(eventData);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                pointerDrag.GetComponent<IDraggableHandler>()?.OnCancelDrag(eventData);
                EndDrag(eventData);
            }
            else// if (eventData.delta.magnitude > 0.1f)
            {
                if(currentArea != null) currentArea.GetComponent<IDropAreaHandler>()?.OnDropAreaMove(eventData);
                pointerDrag.GetComponent<IDraggableHandler>()?.OnDrag(eventData);
            }
        }

        private DragDropEventData CreateEvent()
        {
            var mousePosition = (Vector2)Input.mousePosition;
            var delta = mousePosition - _lastMousePosition;

            return new DragDropEventData
            {
                pointerDrag = pointerDrag,
                dragging = dragging,
                eventCamera = GetComponent<Canvas>()?.worldCamera,
                position = mousePosition,
                delta = delta,
                area = currentArea
            };
        }
    }
}