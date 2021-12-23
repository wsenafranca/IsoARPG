using Player;
using UI;
using UnityEngine;

namespace DialogSystem
{
    public class TriggerMessageBox : MonoBehaviour
    {
        public GameObject messageBoxPrefab;
        public Transform messageBoxPosition;
        
        [TextArea]
        public string text;

        private MessageBox _messageBox;
        
        public void OnPlayerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null) return;

            if (_messageBox == null)
            {
                _messageBox = Instantiate(messageBoxPrefab).GetComponent<MessageBox>();
            }
            
            _messageBox.ShowText(text, messageBoxPosition.position);
        }

        public void OnPlayerExit(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null) return;

            if (_messageBox != null)
            {
                _messageBox.HideText();
            }
        }
    }
}