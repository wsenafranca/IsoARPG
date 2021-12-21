using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Damage;
using UnityEngine;

namespace Weapon
{
    [Serializable]
    public class WeaponMeleeContactPoint
    {
        public Transform root;
        public Vector3 offset;
        public float radius = 0.1f;
    }
    
    public class WeaponMelee : MonoBehaviour
    {
        private LayerMask _layerMask;
        
        [SerializeField]
        private List<WeaponMeleeContactPoint> contactPoints = new();
        
        private bool _isAttacking;
        private Vector3[] _lastPosition;
        private readonly RaycastHit[] _results = new RaycastHit[10];
        private readonly HashSet<GameObject> _hitCharacters = new();
     
        private GameObject _instigator;
        private DamageIntent _damageIntent;
        
        private void Awake()
        {
            _layerMask = LayerMask.GetMask("Character", "Player");
            _lastPosition = new Vector3[contactPoints.Count];
        }

        private void Update()
        {
            if (!_isAttacking) return;
            
            for (var i = 0; i < contactPoints.Count; i++)
            {
                var currentPosition = contactPoints[i].root.position + contactPoints[i].root.TransformVector(contactPoints[i].offset);
                var lastPosition = _lastPosition[i];
                
                var dir = currentPosition - lastPosition;
                if (dir.magnitude < 0.001f)
                {
                    dir = Vector3.forward * 0.0001f;
                }
                dir.Normalize();
                
                var len = Vector3.Distance(currentPosition, lastPosition);
            
                Debug.DrawRay(currentPosition, dir, Color.red, 2.0f);

                var count = Physics.SphereCastNonAlloc(currentPosition, contactPoints[i].radius, dir, _results, len, _layerMask, QueryTriggerInteraction.Ignore);
                for (var j = 0; j < count; j++)
                {
                    var obj = _results[j].collider.gameObject;
                    if (obj == _instigator || _hitCharacters.Contains(obj)) continue;

                    var target = obj.GetComponent<CharacterBase>();
                    if (!target || !target.isAlive) continue;

                    _damageIntent.worldPosition = currentPosition;
                    _damageIntent.normal = dir;
                    target.ApplyDamage(_damageIntent);
                    
                    _hitCharacters.Add(obj);
                }
                
                _lastPosition[i] = currentPosition;
            }
        }

        public void SetDamageIntent(DamageIntent intent)
        {
            _damageIntent = intent;
            _damageIntent.causer = gameObject;
        }

        public void BeginAttack(GameObject instigator)
        {
            _instigator = instigator;
            _isAttacking = true;
            for (var i = 0; i < contactPoints.Count; i++)
            {
                _lastPosition[i] = contactPoints[i].root.position + contactPoints[i].root.TransformVector(contactPoints[i].offset);
            }
            _hitCharacters.Clear();
        }

        public void EndAttack()
        {
            _isAttacking = false;
            _hitCharacters.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);

            foreach (var point in contactPoints.Where(point => point != null && point.root != null))
            {
                var worldPosition = point.root.TransformVector(point.offset);
                Gizmos.DrawSphere(point.root.position + worldPosition, point.radius * transform.lossyScale.x);
            }
        }
#endif
    }
}