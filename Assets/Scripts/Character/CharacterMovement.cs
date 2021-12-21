using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterAnimator))]
    public class CharacterMovement : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private CharacterAnimator _animator;
        private CharacterLookAt _lookAt;

        private Quaternion _targetRotation;
        
        public bool isNavigating => !_agent.isStopped;

        public bool hasReachDestination => Mathf.Abs(_agent.remainingDistance - _agent.stoppingDistance) < 0.1f || _agent.remainingDistance < _agent.stoppingDistance;

        public bool isMoving => _agent.velocity.magnitude > 0.5f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _animator = GetComponent<CharacterAnimator>();
            _lookAt = GetComponent<CharacterLookAt>();
        }

        private void Update()
        {
            var trans = transform;
            if (isMoving)
            {
                var dir = Vector3.Normalize(_agent.nextPosition - trans.position);
                dir.y = 0.0f;
                _targetRotation = Quaternion.LookRotation(dir);
            }

            trans.rotation = Quaternion.RotateTowards(trans.rotation, _targetRotation, _agent.angularSpeed * Time.deltaTime);

            _animator.moving = isMoving;
            _animator.speed = _agent.velocity.magnitude;
            
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

        public void SetDestination(Vector3 target, float acceptableDistance = 0.0f)
        {
            _agent.stoppingDistance = acceptableDistance;
            _agent.SetDestination(target);
            _agent.isStopped = false;
        }

        public float Distance(CharacterMovement character)
        {
            return Mathf.Max(0, Vector3.Distance(transform.position, character.transform.position) - _agent.radius - (character != null ? character._agent.radius : 0));
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
    }
}