using Character;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class AISensing : MonoBehaviour
    {
        public UnityEvent<CharacterBase> targetEnter;
        public UnityEvent<CharacterBase> targetExit;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            targetEnter?.Invoke(other.GetComponent<CharacterBase>());
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            targetExit?.Invoke(other.GetComponent<CharacterBase>());
        }
    }
}