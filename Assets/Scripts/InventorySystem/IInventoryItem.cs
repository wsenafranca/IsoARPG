using System;

namespace InventorySystem
{
    public interface IInventoryItem
    {
        public Guid guid { get; }
        public int itemWidth { get; }
        public int itemHeight { get; }
    }
}