using System.Collections;
using System.Collections.Generic;
using AbilitySystem;
using UnityEngine;
using UnityEngine.Pool;

public class DamageOutputManager : MonoBehaviour
{
    public static DamageOutputManager instance { get; private set; }
    
    public GameObject damagePrefab;
    
    private ObjectPool<GameObject> _pool;
    private readonly Queue<DamageOutputEntry> _queue = new();
    private Transform _cameraTransform;

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
        _cameraTransform = GameObject.FindWithTag("MainCamera").transform;
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
                var damage = obj.GetComponent<DamageOutput>();
                var rotation = Quaternion.LookRotation((entry.location - _cameraTransform.position).normalized);
                damage.SetText(entry.location, rotation, entry.text, entry.color, (dmg)=>_pool.Release(dmg.gameObject));
                obj.SetActive(true);

                if(_queue.Count > 0) yield return new WaitForSeconds(0.2f);
            }
        }
    }

    public void ShowDamage(Vector3 location, DamageInfo damage)
    {
        Color color;
        if(damage.isCritical)
        {
            color = Color.yellow;
        }
        else
        {
            color = damage.damageTarget switch
            {
                DamageTarget.Health => Color.red,
                DamageTarget.Mana => Color.blue,
                DamageTarget.MagicShield => Color.cyan,
                _ => Color.white
            };
        }

        ShowText(location, Mathf.CeilToInt(-damage.damage).ToString(), color);
    }

    public void ShowText(Vector3 location, string text, Color color)
    {
        _queue.Enqueue(new DamageOutputEntry
        {
            location = location,
            text = text,
            color = color
        });
    }

    private struct DamageOutputEntry
    {
        public Vector3 location;
        public string text;
        public Color color;

        public override string ToString()
        {
            return "Damage(" + text + ")";
        }
    }
}