using System;
using UnityEngine;

namespace InventorySystem
{
    public enum EquipmentSlot
    {
        Helm = 0,
        Armor,
        Pants,
        Gloves,
        Boots,
        MainHand,
        OffHand,
        Necklace,
        Ring1,
        Ring2,
        Cape,
        Pet
    }

    [Serializable]
    public struct EquipmentSlotSocket
    {
        public Transform mainHand;
        public Transform offHand;
        public Transform helm;
        public Transform armor;
        public Transform pants;
        public Transform gloves;
        public Transform boots;
        public Transform necklace;
        public Transform ring1;
        public Transform ring2;
        public Transform cape;
        public Transform pet;
    }

    public enum EquipmentRequirement
    {
        Level,
        Strength,
        Stamina,
        Dexterity,
        Intelligence
    }

    [Serializable]
    public struct EquipmentRequirementData
    {
        public EquipmentRequirement requirement;
        public int value;
    }
    
    public enum EquipmentType
    {
        OneHandWeapon,
        TwoHandWeapon,
        Shield,
        Helm,
        Armor,
        Pants,
        Gloves,
        Boots,
        Necklace,
        Ring,
        Cape,
        Pet
    }
    
    public enum EquipmentVisualSlot : byte
    {
        Head = 0,
        Eyebrow,
        Hair,
        Torso,
        UpperArmLeft,
        LowerArmLeft,
        HandLeft,
        UpperArmRight,
        LowerArmRight,
        HandRight,
        Hips,
        LegLeft,
        LegRight
    }

    [Serializable]
    public class EquipmentVisualData
    {
        public EquipmentVisualSlot slot;
        public GameObject prefab;
    }
}