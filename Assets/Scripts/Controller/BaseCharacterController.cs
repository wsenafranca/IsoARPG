using AbilitySystem;
using UnityEngine;
using UnityEngine.AI;

namespace Controller
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class BaseCharacterController : MonoBehaviour
    {
        protected NavMeshAgent agent;
        protected Animator animator;
        protected AbilitySystemComponent abilitySystem;

        public bool isNavigation => agent.pathPending || agent.remainingDistance > agent.stoppingDistance;
        public bool isPlayingAnimation { get; private set; }
        public bool isAlive => abilitySystem.attributeSet.health > 0;

        private Quaternion _targetRotation;
        public bool isLookingAtTarget => Mathf.Abs(_targetRotation.eulerAngles.y - transform.eulerAngles.y) > 1;
    
        private static readonly int MovingHash = Animator.StringToHash("moving");
        private static readonly int SpeedHash = Animator.StringToHash("speed");
        private static readonly int AnimSpeedHash = Animator.StringToHash("animSpeed");

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            abilitySystem = GetComponent<AbilitySystemComponent>();
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
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, agent.angularSpeed * Time.deltaTime);
            }
            else
            {
                _targetRotation = transform.rotation;
            }
            
            animator.SetBool(MovingHash, agent.velocity.magnitude > 0.0f && agent.remainingDistance > agent.radius);
            animator.SetFloat(SpeedHash, Mathf.Round((agent.velocity.magnitude / agent.speed) * 100.0f) / 100.0f);
            animator.SetFloat(AnimSpeedHash, 1.0f);
        }

        public void SetDestination(Vector3 target, float acceptableDistance = 0.0f)
        {
            if (isPlayingAnimation) return;
        
            agent.stoppingDistance = acceptableDistance;
            agent.SetDestination(target);
            agent.isStopped = false;
        }

        public void StopMovement()
        {
            agent.isStopped = true;
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
        
            animator.CrossFade(stateName, 0.1f);
            agent.isStopped = true;
            isPlayingAnimation = true;
        }

        public void StopAnimation()
        {
            isPlayingAnimation = false;
        }
    }
}