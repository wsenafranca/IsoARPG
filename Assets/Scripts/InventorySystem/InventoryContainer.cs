using System;
using System.Collections.Generic;
using Item;

namespace InventorySystem
{
    [Serializable]
    public class InventoryContainer
    {
        public readonly int rows;
        public readonly int columns;
        
        private readonly Guid[,] _grid;
        private readonly Dictionary<Guid, InventoryEntry> _items = new();

        public InventoryContainer(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            
            _grid = new Guid[rows, columns];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    _grid[i, j] = Guid.Empty;
                }
            }
        }

        public bool TryGetItem(int x, int y, out ItemInstance item)
        {
            if (x < 0 || x >= columns || y < 0 || y >= rows)
            {
                item = null;
                return false;
            }

            item = _items[_grid[y, x]].item;
            return item != null;
        }
        
        public bool FindEmptySpace(int width, int height, out int x, out int y)
        {
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    if(!IsEmpty(j, i, width, height)) continue;
                    
                    x = j;
                    y = i;
                    return true;
                }
            }

            x = 0;
            y = 0;
            return false;
        }

        public bool IsValidX(int x)
        {
            return x >= 0 && x < columns;
        }
        
        public bool IsValidY(int y)
        {
            return y >= 0 && y < rows;
        }

        public bool IsValidPoint(int x, int y)
        {
            return IsValidX(x) && IsValidY(y);
        }

        public bool IsValidRect(int x, int y, int width, int height)
        {
            return IsValidPoint(x, y) && width > 0 && width <= columns && height > 0 && height <= rows;
        }

        public bool IsEmpty(int x, int y, int width, int height)
        {
            if (!IsValidRect(x, y, width, height)) return false;
            
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (!IsValidRect(x + j, y + i, width, height) || _grid[y + i, x + j] != Guid.Empty) return false;
                }
            }

            return true;
        }
        
        public bool IsEmpty(int x, int y, ItemInstance item)
        {
            if (!IsValidRect(x, y, item.itemBase.width, item.itemBase.height)) return false;
            
            for (var i = 0; i < item.itemBase.height; i++)
            {
                for (var j = 0; j < item.itemBase.width; j++)
                {
                    if (!IsValidRect(x + j, y + i, item.itemBase.width, item.itemBase.height)) return false;
                    if (_grid[y + i, x + j] != Guid.Empty && _grid[y + i, x + j] != item.guid) return false;
                }
            }

            return true;
        }
        
        public bool AddItem(ItemInstance item, out int x, out int y)
        {
            if (!FindEmptySpace(item.itemBase.width, item.itemBase.height, out x, out y)) return false;
            
            AddItemInternal(item, x, y);

            return true;
        }

        public bool AddItem(ItemInstance item, int x, int y)
        {
            if (item == null || !IsEmpty(x, y, item.itemBase.width, item.itemBase.height)) return false;

            AddItemInternal(item, x, y);
            
            return true;
        }

        private void AddItemInternal(ItemInstance item, int x, int y)
        {
            _items[item.guid] = (new InventoryEntry{x = x, y = y, item = item});
            
            for (var i = y; i < y + item.itemBase.height; i++)
            {
                for (var j = x; j < x + item.itemBase.width; j++)
                {
                    _grid[i, j] = item.guid;
                }
            }
        }

        public bool MoveItem(ItemInstance item, int x, int y)
        {
            if (item == null) return false;

            if (!IsEmpty(x, y, item)) return false;

            if (!CanRemoveItem(item, out _)) return false;
            
            _items.Remove(item.guid);

            AddItemInternal(item, x, y);
            
            return true;
        }
        
        public bool RemoveItem(ItemInstance item, out int x, out int y)
        {
            x = -1;
            y = -1;
            if (!CanRemoveItem(item, out var entry)) return false;

            _items.Remove(item.guid);
            
            x = entry.x;
            y = entry.y;
            return true;
        }
        
        private bool CanRemoveItem(ItemInstance item, out InventoryEntry entry)
        {
            if (!_items.TryGetValue(item.guid, out entry)) return false;
            
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    if (_grid[i, j] != item.guid) continue;
                    _grid[i, j] = Guid.Empty;
                }
            }

            return true;
        }

        private struct InventoryEntry
        {
            public int x;
            public int y;
            public ItemInstance item;
        }

        public override string ToString()
        {
            var ret = "";
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    ret += _grid[i, j] + " ";
                }

                ret += "\n";
            }

            return ret;
        }
    }
}