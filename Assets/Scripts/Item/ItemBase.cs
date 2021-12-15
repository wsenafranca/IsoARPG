using UnityEngine;

namespace Item
{
    public abstract class ItemBase : ScriptableObject
    {
        [Header("Item")]
        public string displayName;
        public string displayDescription;
        public GameObject itemSlotPrefab;
        public int price;
        public int width = 1;
        public int height = 1;
        public ItemCategory category;

        public abstract ItemInstance CreateItemInstance();
    }
}