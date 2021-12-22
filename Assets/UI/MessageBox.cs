using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MessageBox : MonoBehaviour
    {
        public static MessageBox instance { get; private set; }

        public Canvas canvas;
        public Camera worldCamera;
        
        private TextMeshProUGUI _textMesh;
        
        private void Awake()
        {
            _textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            
            gameObject.SetActive(false);
            instance = this;
        }

        public void ShowText(string text, Vector3 worldPosition)
        {
            var canvasRectSize = ((RectTransform) canvas.transform).sizeDelta;
            var screenPoint = worldCamera.WorldToViewportPoint(worldPosition) * canvasRectSize - canvasRectSize * 0.5f;
            
            var rect = GetComponent<RectTransform>();
            
            _textMesh.text = text;
            rect.sizeDelta = new Vector2(_textMesh.preferredWidth + 32, _textMesh.preferredHeight + 32);
            rect.anchoredPosition = screenPoint;

            transform.localScale = Vector3.one * 0.1f;
            gameObject.SetActive(true);
            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one, 0.3f));
        }

        public void HideText()
        {
            transform.localScale = Vector3.one;
            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one * 0.1f, 0.1f))
                .AppendCallback(() => gameObject.SetActive(false));
        }
    }
}