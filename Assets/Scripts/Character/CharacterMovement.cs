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
            var isMoving = _agent.velocity.magnitude > 0.1f;
            if (isMoving)
            {
                var dir = Vector3.Normalize(_agent.nextPosition - trans.position);
                dir.y = 0.0f;
                _targetRotation = Quaternion.LookRotation(dir);
            }

            trans.rotation = Quaternion.RotateTowards(trans.rotation, _targetRotation, _agent.angularSpeed * Time.deltaTime);

            _animator.SetBool(MovingHash, isMoving);
            _animator.SetFloat(SpeedHash, _agent.velocity.magnitude);
            
            if(_lookAt) _lookAt.lookAtTargetPosition = _agent.steeringTarget + transform.forward;
        }

        private void OnAnimatorMove()
        {
            if (_agent.isStopped)
            {
                _agent.nextPosition = transform.position;
            }
            else
            {
                transform.position = _agent.nextPosition;
            }
        }

        public bool SetDestination(Vector3 target, float acceptableDistance = 0.0f)
        {
            if (Vector3.Distance(target, _agent.nextPosition) < acceptableDistance + _agent.radius) return false;
            
            _agent.stoppingDistance = acceptableDistance;
            _agent.SetDestination(target);
            _agent.isStopped = false;
            return true;
        }

        public void StopMovement()
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        public void LookAt(Transform target)
        {
            var dir = Vector3.Normalize(target.position - transform.position);
            dir.y = 0;
            _targetRotation = Quaternion.LookRotation(dir);
        }

        private void FootR()
        {
            
        }

        private void FootL()
        {
            
        }
    }
}