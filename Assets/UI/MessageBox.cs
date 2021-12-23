using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MessageBox : MonoBehaviour
    {
        private TextMeshProUGUI _textMesh;
        private RectTransform _container;
        
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Camera.main) transform.forward = Camera.main.transform.forward;
        }

        public void ShowText(string text, Vector3 worldPosition)
        {
            _container = (RectTransform) transform.Find("Container");
            _textMesh = _container.Find("Text").GetComponent<TextMeshProUGUI>();
            _textMesh.text = text;

            _container.sizeDelta = new Vector2(Mathf.Min(_container.sizeDelta.x, _textMesh.preferredWidth + 16), _textMesh.preferredHeight + 16);

            var trans = (RectTransform)transform;
            trans.position = worldPosition;
            var cam = Camera.main;
            if (cam)
            {
                var camTrans = cam.transform;
                trans.forward = camTrans.forward;
            }
            _container.localScale = Vector3.one * 0.1f;
            gameObject.SetActive(true);
            DOTween.Sequence()
                .Append(_container.DOScale(Vector3.one, 0.3f));
        }

        public void HideText()
        {
            _container.localScale = Vector3.one;
            DOTween.Sequence()
                .Append(_container.DOScale(Vector3.one * 0.1f, 0.1f))
                .AppendCallback(() => gameObject.SetActive(false));
        }
    }
}