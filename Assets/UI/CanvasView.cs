using System;
using AI;
using Character;
using DialogSystem;
using Item;
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

        public void OnPointerEnterTarget(TargetBase target)
        {
            switch (target)
            {
                case Collectible:
                    Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
                    break;
                case Talkative:
                    Cursor.SetCursor(GameAsset.instance.cursorTalk, Vector2.zero, CursorMode.Auto);
                    break;
                case AITarget characterTarget:
                    switch (characterTarget.character.characterType)
                    {
                        case CharacterType.None:
                        case CharacterType.Player:
                        case CharacterType.Ally:
                            Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
                            break;
                        case CharacterType.Enemy:
                            Cursor.SetCursor(GameAsset.instance.cursorAttack, Vector2.zero, CursorMode.Auto);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnPointerExitTarget(TargetBase target)
        {
            Cursor.SetCursor(GameAsset.instance.cursorDefault, Vector2.zero, CursorMode.Auto);
        }
    }
}
