using System;
using AbilitySystem;
using AttributeSystem;
using Character;
using InventorySystem;
using Item;
using Player.Abilities;
using TargetSystem;
using UnityEngine;
using Weapon;

namespace Player
{
    [RequireComponent(typeof(InventoryController))]
    [RequireComponent(typeof(AbilitySystemComponent))]
    [RequireComponent(typeof(CharacterBase))]
    [RequireComponent(typeof(CharacterMovement))]
    public class PlayerController : MonoBehaviour, ITargetSystemInterface, IWeaponMeleeHandler, IAbilitySystemHandler
    {
        public static PlayerController current { get; private set; }
        
        [SerializeField]
        private float minClickDistance = 1.3f;

        private InventoryController _inventory;
        private AbilitySystemComponent _abilitySystem;
        private CharacterBase _character;
        private Animator _animator;
        private CharacterMovement _movement;
        private readonly WeaponMelee[] _weaponMelee = new WeaponMelee[2];
        private int _currentWeaponIndex;
        private static readonly int WeaponTypeHash = Animator.StringToHash("weaponType");
        private static readonly int WeaponIndexHash = Animator.StringToHash("weaponIndex");

        private void Awake()
        {
            _abilitySystem = GetComponent<AbilitySystemComponent>();
            _inventory = GetComponent<InventoryController>();
            _character = GetComponent<CharacterBase>();
            _animator = GetComponent<Animator>();
            _movement = GetComponent<CharacterMovement>();

            current = this;
        }

        private void OnEnable()
        {
            _inventory.onEquip.AddListener(OnEquip);
            _inventory.onUnEquip.AddListener(OnUnEquip);
            _inventory.onAddItem.AddListener(OnAddItem);
            _inventory.onRemoveItem.AddListener(OnRemoveItem);
        }

        private void Update()
        {
            var mainHand = _inventory.GetEquipmentItem(EquipmentSlot.MainHand);
            var offHand = _inventory.GetEquipmentItem(EquipmentSlot.OffHand);

            var weaponType = 0;
            if (offHand is { equipmentType: EquipmentType.Shield })
            {
                weaponType = 1;
            }
            else if (mainHand is {equipmentType:EquipmentType.OneHandWeapon})
            {
                weaponType = 2;
            }
            else if (mainHand is { equipmentType: EquipmentType.TwoHandWeapon })
            {
                weaponType = 3;
            }

            _animator.SetInteger(WeaponTypeHash, weaponType);
            _animator.SetInteger(WeaponIndexHash, _currentWeaponIndex);
        }

        private void OnDisable()
        {
            _inventory.onEquip.RemoveListener(OnEquip);
            _inventory.onUnEquip.RemoveListener(OnUnEquip);
            _inventory.onAddItem.RemoveListener(OnAddItem);
            _inventory.onRemoveItem.RemoveListener(OnRemoveItem);
        }

        public void MoveToHit(RaycastHit hit) => MoveToHit(hit.point);
        
        public void MoveToHit(Vector3 point)
        {
            if (_character.isPlayingAnimation) return;
            
            _abilitySystem.DeactivateAllAbilities();

            if (Vector3.Distance(point, transform.position) < minClickDistance) return;

            _movement.SetDestination(point);
        }

        public void MoveToTarget(Targetable target)
        {
            if (_abilitySystem.isAnyAbilityActive || _character.isPlayingAnimation) return;

            if (!target) return;

            var result = AbilityActivateResult.NotFound;
            switch (target.targetType)
            {
                case TargetType.Neutral:
                    break;
                case TargetType.Enemy:
                    if (_abilitySystem.TryGetAbility<MeleeAttackAbility>(out var ability))
                    {
                        ability.weaponIndex = _currentWeaponIndex;
                        if (_inventory.TryGetEquipmentItem(EquipmentSlot.OffHand, out var item) && item.equipmentType == EquipmentType.OneHandWeapon)
                        {
                            _currentWeaponIndex = (_currentWeaponIndex + 1) % 2;
                        }
                        else
                        {
                            _currentWeaponIndex = 0;
                        }
                    }
                    result = _abilitySystem.TryActivateAbility<MeleeAttackAbility>();
                    break;
                case TargetType.Talkative:
                    
                    break;
                case TargetType.Collectible:
                    result = _abilitySystem.TryActivateAbility<CollectAbility>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (result)
            {
                case AbilityActivateResult.Success:
                    break;
                case AbilityActivateResult.NotEnoughResource:
                    Debug.Log("Not enough resource");
                    break;
                case AbilityActivateResult.NotFound:
                    Debug.Log("Not found");
                    break;
                case AbilityActivateResult.AlreadyActivate:
                    Debug.Log("Already activate");
                    break;
                case AbilityActivateResult.NotReady:
                    Debug.Log("Not ready");
                    break;
                case AbilityActivateResult.CannotUseAbility:
                    Debug.Log("Cannot use ability");
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
        
        public Targetable GetCurrentTarget()
        {
            return InputController.instance.currentTarget;
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
                if (_character.attributeSet.TryGetAttribute(attribute.attribute, out var value))
                {
                    value.AddModifier(new AdditiveAttributeModifier(attribute.value.currentValue, equipment));
                }
            }
            
            foreach (var modifier in equipment.additiveModifiers)
            {
                if (_character.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.AddModifier(new AdditiveAttributeModifier(modifier.value, equipment));
                }
            }
            
            foreach (var modifier in equipment.multiplicativeModifiers)
            {
                if (_character.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.AddModifier(new MultiplicativeAttributeModifier(modifier.value, equipment));
                }
            }

            switch (slot)
            {
                case EquipmentSlot.MainHand:
                {
                    _inventory.TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[0] = itemObj.GetComponent<WeaponMelee>();
                    break;
                }
                case EquipmentSlot.OffHand:
                {
                    _inventory.TryGetAttachedParts(slot, out var itemObj);
                    _weaponMelee[1] = itemObj.GetComponent<WeaponMelee>();
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
                if (_character.attributeSet.TryGetAttribute(attribute.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            foreach (var modifier in equipment.additiveModifiers)
            {
                if (_character.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            foreach (var modifier in equipment.multiplicativeModifiers)
            {
                if (_character.attributeSet.TryGetAttribute(modifier.attribute, out var value))
                {
                    value.RemoveAllModifier(equipment);
                }
            }
            
            switch (slot)
            {
                case EquipmentSlot.MainHand:
                {
                    _inventory.TryGetAttachedParts(slot, out _);
                    _weaponMelee[0] = null;
                    break;
                }
                case EquipmentSlot.OffHand:
                {
                    _inventory.TryGetAttachedParts(slot, out _);
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

        public WeaponMelee GetWeaponMeleeController(int weaponIndex)
        {
            return _weaponMelee[weaponIndex];
        }

        public bool ConsumeAbilityResource(AbilityBase abilityBase)
        {
            switch (abilityBase.cost.resource)
            {
                case AbilityCostResource.Health:
                    if (_character.health < abilityBase.cost.value) return false;
                    _character.health -= abilityBase.cost.value;
                    return true;
                case AbilityCostResource.Mana:
                    if (_character.mana < abilityBase.cost.value) return false;
                    _character.mana -= abilityBase.cost.value;
                    return true;
                case AbilityCostResource.EnergyShield:
                    if (_character.energyShield < abilityBase.cost.value) return false;
                    _character.energyShield -= abilityBase.cost.value;
                    return true;
                case AbilityCostResource.Gold:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
