using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Animator))]
    public class CharacterLookAt : MonoBehaviour
    {
        public Transform head = null;
        public Vector3 lookAtTargetPosition;
        public float lookAtCoolTime = 0.2f;
        public float lookAtHeatTime = 0.2f;
        public bool looking = true;
        
        private Vector3 _lookAtPosition;
        private Animator _animator;
        private float _lookAtWeight;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            lookAtTargetPosition = head.position + transform.forward;
            _lookAtPosition = lookAtTargetPosition;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (head == null) return;

            var headPosition = head.position;
            
            lookAtTargetPosition.y = headPosition.y;
            var lookAtTargetWeight = looking ? 1.0f : 0.0f;

            var curDir = _lookAtPosition - head.position;
            var futDir = lookAtTargetPosition - headPosition;

            curDir = Vector3.RotateTowards(curDir, futDir, 6.28f*Time.deltaTime, float.PositiveInfinity);
            _lookAtPosition = headPosition + curDir;

            var blendTime = lookAtTargetWeight > _lookAtWeight ? lookAtHeatTime : lookAtCoolTime;
            _lookAtWeight = Mathf.MoveTowards (_lookAtWeight, lookAtTargetWeight, Time.deltaTime/blendTime);
            _animator.SetLookAtWeight (_lookAtWeight, 0.2f, 0.5f, 0.7f, 0.5f);
            _animator.SetLookAtPosition (_lookAtPosition);
        }
    }
}