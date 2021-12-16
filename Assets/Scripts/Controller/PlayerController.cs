using System;
using AbilitySystem.Abilities;
using InventorySystem;
using Item;
using TargetSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controller
{
    [RequireComponent(typeof(InventoryController))]
    public class PlayerController : BaseCharacterController, ITargetSystemInterface
    {
        public static PlayerController instance { get; private set; }
        
        [SerializeField]
        private float minClickDistance = 0.1f;
        
        public InventoryController inventory { get; private set; }

        private Targetable _currentTarget;
        public Targetable currentTarget
        {
            get => _currentTarget;
            set
            {
                if (currentTarget == value) return;
                _currentTarget = value;
            }
        }

        [Header("Cheat")] 
        public ItemBase[] genItems;

        protected override void Awake()
        {
            base.Awake();

            inventory = GetComponent<InventoryController>();
            
            instance = this;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            inventory.onEquip.AddListener(OnEquip);
            inventory.onUnEquip.AddListener(OnUnEquip);
            inventory.onAddItem.AddListener(OnAddItem);
            inventory.onRemoveItem.AddListener(OnRemoveItem);
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                var item = genItems[Random.Range(0, genItems.Length)];
                var itemDrop = Instantiate(item.itemSlotPrefab);
                GroundController.instance.GetGroundPosition(Input.mousePosition, out var pos);
                itemDrop.GetComponent<Collectible>().SetAsDrop(item, null, pos);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            inventory.onEquip.RemoveListener(OnEquip);
            inventory.onUnEquip.RemoveListener(OnUnEquip);
            inventory.onAddItem.RemoveListener(OnAddItem);
            inventory.onRemoveItem.RemoveListener(OnRemoveItem);
        }
    
        public Targetable GetCurrentTarget()
        {
            return _currentTarget;
        }

        public void MoveToHit(RaycastHit hit) => MoveToHit(hit.point);
        
        public void MoveToHit(Vector3 point)
        {
            abilitySystem.DeactivateAllAbilities();

            if (Vector3.Distance(point, transform.position) < minClickDistance) return;

            SetDestination(point);
        }

        public void MoveToTarget(Targetable target)
        {
            if (abilitySystem.isAnyAbilityActive) return;

            _currentTarget = target;
            if (!_currentTarget) return;

            var result = AbilityActivateResult.NotFound;
            switch (_currentTarget.targetType)
            {
                case TargetType.Neutral:
                    break;
                case TargetType.Enemy:
                    result = abilitySystem.TryActivateAbility<MeleeAttackAbility>();
                    break;
                case TargetType.Talkative:
                    
                    break;
                case TargetType.Collectible:
                    result = abilitySystem.TryActivateAbility<CollectAbility>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (result)
            {
                case AbilityActivateResult.Success:
                    break;
                case AbilityActivateResult.NotEnoughResource:
                    Debug.Log("NotEnoughResource");
                    break;
                case AbilityActivateResult.NotFound:
                    Debug.Log("NotFound");
                    break;
                case AbilityActivateResult.AlreadyActivate:
                    Debug.Log("AlreadyActivate");
                    break;
                case AbilityActivateResult.NotReady:
                    Debug.Log("NotReady");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnWeaponBeginAttack(int weaponIndex)
        {
            inventory.GetWeaponMeleeController(weaponIndex)?.BeginAttack(gameObject);
        }

        private void OnWeaponEndAttack(int weaponIndex)
        {
            inventory.GetWeaponMeleeController(weaponIndex)?.EndAttack();
        }

        public void OnAddItem(ItemInstance item, int x, int y)
        {
        }
        
        public void OnRemoveItem(ItemInstance item, int x, int y)
        {
            
        }
        
        public void OnEquip(EquipmentItemInstance item, EquipmentSlot slot)
        {
        }
        
        public void OnUnEquip(EquipmentItemInstance item, EquipmentSlot slot)
        {
        }

        private void OnGUI()
        {
        }
    }
}
