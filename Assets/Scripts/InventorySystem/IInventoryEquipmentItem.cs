using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public interface IInventoryEquipmentItem
    {
        public EquipmentType equipmentType { get; }

        public IEnumerable<EquipmentVisualData> visualItemPrefab { get; }

        public GameObject attachItemPrefab { get; }

        public IEnumerable<EquipmentRequirementData> requirements { get; }
    }
}