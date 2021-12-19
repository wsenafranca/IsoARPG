using Character;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterMeterBar : MonoBehaviour
    {
        private Image _meterBar;

        private void Awake()
        {
            _meterBar = GetComponent<Image>();
        }

        public void OnCharacterValueChanged(CharacterBase character, int currentValue, int maxValue)
        {
            if(_meterBar) _meterBar.fillAmount = Mathf.Clamp01(((float)currentValue) / maxValue);
        }
    }
}