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
            targetEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            targetExit?.Invoke(other);
        }
    }
}