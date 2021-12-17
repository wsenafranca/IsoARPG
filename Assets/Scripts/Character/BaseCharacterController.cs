using AttributeSystem;
using UnityEngine;
using UnityEngine.AI;

namespace Controller
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AttributeSet))]
    public class BaseCharacterController : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Animator _animator;
        protected AttributeSet attributeSet;

        public bool isNavigation => _agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance;
        public bool isPlayingAnimation { get; private set; }
        public bool isAlive => attributeSet.health > 0;

        private Quaternion _targetRotation;
        public bool isLookingAtTarget => Mathf.Abs(_targetRotation.eulerAngles.y - transform.eulerAngles.y) > 1;
    
        private static readonly int MovingHash = Animator.StringToHash("moving");
        private static readonly int SpeedHash = Animator.StringToHash("speed");
        private static readonly int AnimSpeedHash = Animator.StringToHash("animSpeed");

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            attributeSet = GetComponent<AttributeSet>();
        }

        protected virtual void OnEnable()
        {
            _targetRotation = transform.rotation;
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Update()
        {
            if (!isNavigation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _agent.angularSpeed * Time.deltaTime);
            }
            else
            {
                _targetRotation = transform.rotation;
            }
            
            _animator.SetBool(MovingHash, _agent.velocity.magnitude > 0.0f && _agent.remainingDistance > _agent.radius);
            _animator.SetFloat(SpeedHash, Mathf.Round((_agent.velocity.magnitude / _agent.speed) * 100.0f) / 100.0f);
            _animator.SetFloat(AnimSpeedHash, 1.0f);
        }

        public void SetDestination(Vector3 target, float acceptableDistance = 0.0f)
        {
            if (isPlayingAnimation) return;
        
            _agent.stoppingDistance = acceptableDistance;
            _agent.SetDestination(target);
            _agent.isStopped = false;
        }

        public void StopMovement()
        {
            _agent.isStopped = true;
        }

        public void LookAt(Vector3 target)
        {
            var forward = (target - transform.position);
            forward.y = 0.0f;
            _targetRotation = Quaternion.LookRotation(forward.normalized);
        }
        
        public void LookAt(Transform target)
        {
            var forward = (target.position - transform.position);
            forward.y = 0.0f;
            _targetRotation = Quaternion.LookRotation(forward.normalized);
        }

        public void PlayAnimation(string stateName)
        {
            if (isPlayingAnimation) return;
        
            _animator.CrossFade(stateName, 0.1f);
            _agent.isStopped = true;
            isPlayingAnimation = true;
        }

        public void StopAnimation()
        {
            isPlayingAnimation = false;
        }
    }
}