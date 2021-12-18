using System;
using TargetSystem;
using UnityEngine;

namespace UI
{
    public class CanvasView : MonoBehaviour
    {
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
        }

        public void OnPointerEnterTarget(Targetable target)
        {
            switch (target.targetType)
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
        }

        public void OnPointerExitTarget(Targetable target)
        {
            Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
        }
    }
}
