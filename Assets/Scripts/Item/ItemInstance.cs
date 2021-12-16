using System;
using InventorySystem;
using UnityEngine;

namespace Item
{
    [Serializable]
    public class ItemInstance : IInventoryItem
    {
        public ItemBase itemBase;

        public ItemInstance(Guid guid)
        {
            this.guid = guid;
        }

        public T GetItemBase<T>() where T : ItemBase => itemBase as T;
        
        public virtual Color itemColor => Color.white;

        public Guid guid { get; }

        public int itemWidth => itemBase.width;
        public int itemHeight => itemBase.height;
    }
}