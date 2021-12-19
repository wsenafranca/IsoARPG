﻿using AttributeSystem;
using Character;
using TargetSystem;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TargetView : MonoBehaviour
    {
        private TextMeshProUGUI _targetName;
        private CharacterMeterBar _healthBar;

        private void Awake()
        {
            _targetName = transform.Find("TargetName").GetComponent<TextMeshProUGUI>();
            _healthBar = transform.Find("HealthBar").GetComponent <CharacterMeterBar>();
            gameObject.SetActive(false);
        }

        public void OnPointerEnterTarget(Targetable target)
        {
            var character = target.GetComponent<CharacterBase>();
            if (!character || !character.isAlive) return;

            character.currentHealthChanged.AddListener(_healthBar.OnCharacterValueChanged);
            character.death.AddListener(OnCharacterDeath);
            
            _healthBar.OnCharacterValueChanged(character, character.currentHealth, character.attributeSet.GetAttributeValueOrDefault(Attribute.MaxHealth));
            
            _targetName.text = character.displayName;
            gameObject.SetActive(true);
        }

        public void OnPointerExitTarget(Targetable target)
        {
            gameObject.SetActive(false);

            if (!target) return;
            
            var character = target.GetComponent<CharacterBase>();
            if (!character) return;
            
            character.currentHealthChanged.RemoveListener(_healthBar.OnCharacterValueChanged);
            character.death.RemoveListener(OnCharacterDeath);
        }

        private void OnCharacterDeath(CharacterBase character)
        {
            gameObject.SetActive(false);
            character.currentHealthChanged.RemoveListener(_healthBar.OnCharacterValueChanged);
            character.death.RemoveListener(OnCharacterDeath);
        }
    }
}