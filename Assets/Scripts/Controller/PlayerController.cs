using System;
using AbilitySystem.Abilities;
using AttributeSystem;
using InventorySystem;
using Item;
using TargetSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controller
{
    [RequireComponent(typeof(InventoryController))]
    public class PlayerController : BaseCharacterController, ITargetSystemInterface, IWeaponMeleeControllerInterface
    {
        public static PlayerController instance { get; private set; }
        
        [SerializeField]
        private float minClickDistance = 0.1f;
        
        public InventoryController inventory { get; private set; }
        private readonly WeaponMeleeController[] _weaponMelee = new WeaponMeleeController[2];

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
            if(_weaponMelee[weaponIndex]) _weaponMelee[weaponIndex].BeginAttack(gameObject);
        }

        private void OnWeaponEndAttack(int weaponIndex)
        {
            if(_weaponMelee[weaponIndex]) _weaponMelee[weaponIndex].EndAttack();
        }

        public void OnAddItem(IInventoryItem item, int x, int y)
        {
        }
        
        public void OnRemoveItem(IInventoryItem item, int x, int y)
        {
            
        }
        
        public void OnEquip(IInventoryEquipmentItem item, EquipmentSlot slot)
        {
            var equipment = (EquipmentItemInstance)item;
            if (equipment == null) return;

            foreach (var attribute in equipment.attributes)
            {
                if (abilitySystem.attributeSet.TryGetAttribute(attribute.attribute, out var value))
                {
                    value.AddModifier(new AdditiveAttributeModifier(attribute.value.currentValue, equipment));
                }
            }
            
            foreach (var modifier in equipment.additiveModifiers)
            {
                if (abilitySystem.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.AddModifier(new AdditiveAttributeModifier(modifier.value, equipment));
                }
            }
            
            foreach (var modifier in equipment.multiplicativeModifiers)
            {
                if (abilitySystem.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.AddModifier(new MultiplicativeAttributeModifier(modifier.value, equipment));
                }
            }

            switch (slot)
            {
                case EquipmentSlot.MainHand:
                {
                    inventory.TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[0] = itemObj.GetComponent<WeaponMeleeController>();
                    break;
                }
                case EquipmentSlot.OffHand:
                {
                    inventory.TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[1] = itemObj.GetComponent<WeaponMeleeController>();
                    break;
                }
                case EquipmentSlot.Helm:
                case EquipmentSlot.Armor:
                case EquipmentSlot.Pants:
                case EquipmentSlot.Gloves:
                case EquipmentSlot.Boots:
                case EquipmentSlot.Necklace:
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                case EquipmentSlot.Cape:
                case EquipmentSlot.Pet:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }
        
        public void OnUnEquip(IInventoryEquipmentItem item, EquipmentSlot slot)
        {
            var equipment = (EquipmentItemInstance)item;
            if (equipment == null) return;
            
            foreach (var attribute in equipment.attributes)
            {
                if (abilitySystem.attributeSet.TryGetAttribute(attribute.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            foreach (var modifier in equipment.additiveModifiers)
            {
                if (abilitySystem.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            foreach (var modifier in equipment.multiplicativeModifiers)
            {
                if (abilitySystem.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            switch (slot)
            {
                case EquipmentSlot.MainHand:
                {
                    inventory.TryGetAttachedParts(slot, out _);
                    _weaponMelee[0] = null;
                    break;
                }
                case EquipmentSlot.OffHand:
                {
                    inventory.TryGetAttachedParts(slot, out _);
                    _weaponMelee[1] = null;
                    break;
                }
                case EquipmentSlot.Helm:
                case EquipmentSlot.Armor:
                case EquipmentSlot.Pants:
                case EquipmentSlot.Gloves:
                case EquipmentSlot.Boots:
                case EquipmentSlot.Necklace:
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                case EquipmentSlot.Cape:
                case EquipmentSlot.Pet:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        public WeaponMeleeController GetWeaponMeleeController(int weaponIndex)
        {
            return _weaponMelee[weaponIndex];
        }
    }
}
