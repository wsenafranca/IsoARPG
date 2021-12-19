using System;
using System.Collections.Generic;
using Character;
using Damage;
using UnityEngine;

namespace Weapon
{
    public interface IWeaponMeleeHandler
    {
        public WeaponMelee GetWeaponMelee(int weaponIndex);
    }
    
    [Serializable]
    public class WeaponMeleeContactPoint
    {
        public Transform point1;
        public Transform point2;
        public float radius = 0.1f;
    }
    
    public class WeaponMelee : MonoBehaviour
    {
        [SerializeField]
        private WeaponMeleeContactPoint contactPoint = new();
        
        [SerializeField]
        private int collisionSteps = 4;
        
        private bool _isAttacking;
        private Vector3 _lastContactPoint1;
        private Vector3 _lastContactPoint2;
        private readonly RaycastHit[] _results = new RaycastHit[10];
        private readonly HashSet<GameObject> _hitCharacters = new();
     
        private GameObject _instigator;
        private DamageIntent _damageIntent;

        private void Update()
        {
            if (!_isAttacking) return;

            var layerMask = LayerMask.GetMask("Character", "Player");
            var step = 1.0f / collisionSteps;
            for (var alpha = 0.0f; alpha <= 1.0f; alpha += step)
            {
                var point1  = Vector3.Lerp(_lastContactPoint1, contactPoint.point1.position, alpha);
                var point2 = Vector3.Lerp(_lastContactPoint2, contactPoint.point2.position, alpha);
                var dir = (point2 - point1).normalized;
                var len = Vector3.Distance(point1, point2);
                
                var count = Physics.SphereCastNonAlloc(point1, contactPoint.radius, dir, _results, len, layerMask, QueryTriggerInteraction.Ignore);
                for (var i = 0; i < count; i++)
                {
                    var obj = _results[i].collider.gameObject;
                    if (_hitCharacters.Contains(obj) || obj == _instigator) continue;

                    var target = obj.GetComponent<CharacterBase>();
                    if (!target || !target.isAlive) continue;

                    _damageIntent.worldPosition = (point1 + point2) * 0.5f;
                    target.ApplyDamage(_damageIntent);
                    
                    _hitCharacters.Add(obj);
                }
            }

            _lastContactPoint1 = contactPoint.point1.position;
            _lastContactPoint2 = contactPoint.point2.position;
        }

        public void SetDamageIntent(DamageIntent intent)
        {
            _damageIntent = intent;
        }

        public void BeginAttack(GameObject instigator)
        {
            _instigator = instigator;
            _isAttacking = true;
            _lastContactPoint1 = contactPoint.point1.position;
            _lastContactPoint2 = contactPoint.point2.position;
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
            if (!contactPoint.point1 || !contactPoint.point2) return;

            var point1Position = contactPoint.point1.position;
            var point2Position = contactPoint.point2.position;
            Gizmos.DrawLine(point1Position, point2Position);
            var scale = transform.lossyScale.x;
            Gizmos.DrawSphere(point1Position, contactPoint.radius * scale);
            Gizmos.DrawSphere(point2Position, contactPoint.radius * scale);
        }
#endif
    }
}