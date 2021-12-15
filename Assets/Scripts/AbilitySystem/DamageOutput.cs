using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace AbilitySystem
{
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageOutput : MonoBehaviour
    {
        private TextMeshPro _textMesh;
        private Vector3 _location;
        private Quaternion _rotation;
        private string _text;
        private Color _color;
        private Action<DamageOutput> _onFinishedAction;
        
        private void Awake()
        {
            _textMesh = GetComponent<TextMeshPro>();
        }
        
        private void OnEnable()
        {
            transform.SetPositionAndRotation(_location, _rotation);
            _textMesh.text = _text;
            _textMesh.color = _color;
            
            _textMesh.alpha = 1.0f;
            transform.localScale = Vector3.zero;
            DOTween.Sequence()
                .Append(transform.DOScale(1.0f, 0.1f))
                .Append(transform.DOMove(_location + Vector3.up, 0.4f))
                .Append(_textMesh.DOFade(0.0f, 0.5f))
                .AppendCallback(() =>
                {
                    _onFinishedAction?.Invoke(this);
                    _onFinishedAction = null;
                });
        }

        public void SetText(Vector3 location, Quaternion rotation, string text, Color color, Action<DamageOutput> onFinishedAction)
        {
            _location = location;
            _rotation = rotation;
            _text = text;
            _color = color;
            _onFinishedAction = onFinishedAction;
        }
    }
}