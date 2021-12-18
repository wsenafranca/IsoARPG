using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class CharacterMovement : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Animator _animator;
        private CharacterLookAt _lookAt;

        private Quaternion _targetRotation;
        
        public bool isNavigation => _agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance;
        
        private static readonly int MovingHash = Animator.StringToHash("moving");
        private static readonly int SpeedHash = Animator.StringToHash("speed");

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _animator = GetComponent<Animator>();
            _lookAt = GetComponent<CharacterLookAt>();
        }

        private void Update()
        {
            var trans = transform;
            var isMoving = _agent.velocity.magnitude > 0.0f;
            if (isMoving)
            {
                _targetRotation = Quaternion.LookRotation(Vector3.Normalize(_agent.nextPosition - trans.position));
            }

            trans.rotation = Quaternion.RotateTowards(trans.rotation, _targetRotation, _agent.angularSpeed * Time.deltaTime);

            _animator.SetBool(MovingHash, isMoving);
            _animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
            
            if(_lookAt) _lookAt.lookAtTargetPosition = _agent.steeringTarget + transform.forward;
        }

        private void OnAnimatorMove()
        {
            if(isNavigation) transform.position = _agent.nextPosition;
        }

        public void SetDestination(Vector3 target, float acceptableDistance = 0.0f)
        {
            _agent.stoppingDistance = acceptableDistance;
            _agent.SetDestination(target);
            _agent.isStopped = false;
        }

        public void StopMovement()
        {
            _agent.isStopped = true;
            _agent.nextPosition = transform.position;
        }

        public void LookAt(Transform target)
        {
            _targetRotation = Quaternion.LookRotation(Vector3.Normalize(target.position - transform.position));
        }

        private void FootR()
        {
            
        }

        private void FootL()
        {
            
        }
    }
}