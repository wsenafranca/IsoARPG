using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Pool;

namespace Damage
{
    public class DamageOutputManager : MonoBehaviour
    {
        public static DamageOutputManager instance { get; private set; }
    
        public GameObject damagePrefab;
        public Camera worldCamera;
        public Canvas canvas;
    
        private ObjectPool<GameObject> _pool;
        private readonly Queue<DamageOutputEntry> _queue = new();
    
        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                ()=>Instantiate(damagePrefab), 
                null, 
                obj => obj.SetActive(false), 
                Destroy, true, 20, 100);
            _pool.Release(_pool.Get()); // warming

            instance = this;
        }

        private void OnEnable()
        {
            StartCoroutine(ShowDamage_());
        }

        private IEnumerator ShowDamage_()
        {
            while (enabled)
            {
                yield return new WaitUntil(() => _queue.Count > 0);

                while (_queue.TryDequeue(out var entry))
                {
                    var obj = _pool.Get();
                    if (obj == null) continue;
            
                    obj.SetActive(false);
                
                    obj.transform.SetParent(canvas.transform, false);

                    var canvasRect = canvas.GetComponent<RectTransform>();
                    var viewportPoint = worldCamera.WorldToViewportPoint(entry.worldPosition) - new Vector3(0.5f, 0.5f, 0.0f);
                
                    var damage = obj.GetComponent<DamageView>();
                    damage.text = entry.text;
                    damage.color = entry.color;
                    damage.finishedCallback = _pool.Release;
                    damage.position = Vector2.Scale(viewportPoint, canvasRect.sizeDelta);
                
                    obj.SetActive(true);
                
                    if(_queue.Count > 0) yield return new WaitForSeconds(0.2f);
                }
            }
        }

        public void ShowDamage(DamageHit damage)
        {
            if(damage.criticalHit)
            {
                ShowText(damage.worldPosition, $"Critical!\n{-damage.value}", GameAsset.instance.criticalHit);
                return;
            }

            if (damage.blockedHit)
            {
                ShowText(damage.worldPosition, "Block", GameAsset.instance.criticalHit);
                return;
            }
        
            if (damage.missedHit)
            {
                ShowText(damage.worldPosition, "Miss", GameAsset.instance.criticalHit);
                return;
            }

            var color = damage.damageType switch
            {
                DamageType.Health => GameAsset.instance.healthHit,
                DamageType.Mana => GameAsset.instance.manaHit,
                DamageType.EnergyShield => GameAsset.instance.energyShieldHit,
                _ => throw new ArgumentOutOfRangeException()
            };

            ShowText(damage.worldPosition, (-damage.value).ToString(), color);
        }

        public void ShowText(Vector3 worldPosition, string text, VertexGradient color)
        {
            _queue.Enqueue(new DamageOutputEntry
            {
                worldPosition = worldPosition,
                text = text,
                color = color
            });
        }

        private struct DamageOutputEntry
        {
            public Vector3 worldPosition;
            public string text;
            public VertexGradient color;

            public override string ToString()
            {
                return "Damage(" + text + ")";
            }
        }
    }
}