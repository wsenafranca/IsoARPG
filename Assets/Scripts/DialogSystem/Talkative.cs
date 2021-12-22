using Player;
using UI;
using UnityEngine;

namespace DialogSystem
{
    public class Talkative : TargetBase
    {
        public Transform messageBoxPosition;
        
        [TextArea]
        public string text;
        
        public override Color outlineColor => GameAsset.instance.outlineTalkative;

        public void OnPlayerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            
            MessageBox.instance.ShowText(text, messageBoxPosition);
        }

        public void OnPlayerExit(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            
            MessageBox.instance.HideText();
        }

        public override bool isTargetValid => true;
    }
}