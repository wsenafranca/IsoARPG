using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DamageView : MonoBehaviour
    {
        [HideInInspector]
        public string text;
        [HideInInspector]
        public VertexGradient color;
        [HideInInspector] 
        public Vector2 position;
        
        public Action<GameObject> finishedCallback;
        
        private TextMeshProUGUI _textMesh;
        private RectTransform _rect;

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(text))
            {
                finishedCallback?.Invoke(gameObject);
                return;
            }
            
            _textMesh = GetComponent<TextMeshProUGUI>();
            
            _textMesh.text = text;
            _textMesh.colorGradient = color;
            _textMesh.alpha = 1.0f;
            _textMesh.enableVertexGradient = true;

            _rect = GetComponent<RectTransform>();
            _rect.localPosition = position;
            
            _rect.localScale = Vector3.one * 0.1f;
            DOTween.Sequence()
                .Join(DOTween.Sequence().AppendInterval(0.5f).Append(_textMesh.DOFade(0.0f, 0.5f)))
                .Join(_rect.DOAnchorPosY(_rect.anchoredPosition.y + 32, 0.3f))
                .Join(_rect.DOScale(Vector3.one, 0.4f))
                .AppendCallback(()=>finishedCallback(gameObject));
        }

        private void OnDisable()
        {
            finishedCallback = null;
        }
    }
}
