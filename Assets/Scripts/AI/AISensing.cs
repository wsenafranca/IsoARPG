using Character;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class AISensing : MonoBehaviour
    {
        public UnityEvent<Collider> targetEnter;
        public UnityEvent<Collider> targetExit;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            targetEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == transform.parent.gameObject) return;

            targetExit?.Invoke(other);
        }
    }
}