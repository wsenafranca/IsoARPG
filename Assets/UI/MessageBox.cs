using System;
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
        private Transform _position;
        private bool _isShowingText;
        
        private void Awake()
        {
            _textMesh = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            
            gameObject.SetActive(false);
            instance = this;
        }

        private void Update()
        {
            if (!_isShowingText) return;
            
            var canvasRectSize = ((RectTransform) canvas.transform).sizeDelta;
            var screenPoint = worldCamera.WorldToViewportPoint(_position.position) * canvasRectSize - canvasRectSize * 0.5f;

            ((RectTransform)transform).anchoredPosition = screenPoint;
        }

        public void ShowText(string text, Transform position)
        {
            _position = position;
            var canvasRectSize = ((RectTransform) canvas.transform).sizeDelta;
            var screenPoint = worldCamera.WorldToViewportPoint(_position.position) * canvasRectSize - canvasRectSize * 0.5f;
            
            var rect = (RectTransform)transform;
            
            _textMesh.text = text;
            rect.sizeDelta = new Vector2(_textMesh.preferredWidth + 32, _textMesh.preferredHeight + 32);
            rect.anchoredPosition = screenPoint;

            _isShowingText = true;
            rect.localScale = Vector3.one * 0.1f;
            gameObject.SetActive(true);
            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one, 0.3f));
        }

        public void HideText()
        {
            _isShowingText = false;
            transform.localScale = Vector3.one;
            DOTween.Sequence()
                .Append(transform.DOScale(Vector3.one * 0.1f, 0.1f))
                .AppendCallback(() => gameObject.SetActive(false));
        }
    }
}