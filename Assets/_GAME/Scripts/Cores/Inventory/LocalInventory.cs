using System;
using System.Collections.Generic;
using System.Linq;

namespace _GAME.Scripts.Inventory
{
    [Serializable]
    public class LocalInventory
    {
        private Dictionary<string, int> _counts;
        private InventoryManager _inventoryManager;
        private List<ItemAmount> _outItems;
        public int Capacity { get; protected set; }

        public LocalInventory(int capacity =-1 )
        {
            Capacity =  capacity;
            _counts = new Dictionary<string, int>(StringComparer.Ordinal);
            _outItems = new();
            _inventoryManager = Core.Get<InventoryManager>();
        }

        public List<ItemAmount> GetLootItems()
        {
            var lootItems = new List<ItemAmount>();
            foreach (var itemAmount in _counts)
            {
                if (itemAmount.Value > 0 && _inventoryManager.ItemDatabase.TryGet(itemAmount.Key, out var itemData))
                {
                    lootItems.Add(new ItemAmount()
                    {
                        Item = itemData,
                        Amount = itemAmount.Value
                    });
                }
            }
            
            return lootItems;
        }

        public List<ItemAmount> GetStorageItems() => _outItems.ToList();
        
        public int GetCount(string id)
        {
            return _counts.TryGetValue(id, out var c) ? c : 0;
        }
        
        public int GetCount(ItemInfo item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id))
                return 0;

            return _counts.TryGetValue(item.Id, out var c) ? c : 0;
        }
        
        public bool Add(ItemAmount amount)
        {
            return Add(amount.Item, amount.Amount);
        }

        public bool Add(List<ItemAmount> amounts)
        {
            bool result = amounts.Count>0;
            foreach (var amount in amounts)
            {
                if (!Add(amount))
                    result = false;
            }
            return result;
        }

        public bool Add(ItemInfo item, int amount)
        {
            if (item == null) return false;
            if (amount == 0) return false;
            if (string.IsNullOrWhiteSpace(item.Id)) return false;

            var current = GetCount(item);
            
            var target = current + amount;
            
            if (target < 0) return false;

            if (target == 0) _counts.Remove(item.Id);
            else _counts[item.Id] = target;
            return true;
        }

        public bool Add(string id, int amount)
        {
            if (amount != 0 && _inventoryManager.ItemDatabase.TryGet(id, out var item))
            {
                return Add(item, amount);
            }

            return false;
        }
        
        public bool Remove(string id, int amount)
        {
            return Add(id, -amount);
        }
        
        public bool Remove(ItemInfo item, int amount)
        {
            return Add(item, -amount);
        }

        public bool Remove(ItemAmount amount)
        {
            return Remove(amount.Item, amount.Amount);
        }
        
        public bool Remove(List<ItemAmount> amounts)
        {
            bool result = amounts.Count>0;
            foreach (var amount in amounts)
            {
                if (!Remove(amount))
                    result = false;
            }
            return result;
        }

        public void TryToOut()
        {
            foreach (var itemAmount in _counts)
            {
                TryToOut(itemAmount.Key, out var isFull);
                if (isFull) break;
            }
        }

        public bool TryToOut(int index)
        {
            int i = 0;
            foreach (var itemAmount in _counts)
            {
                if(i == index)
                {
                    if(itemAmount.Value>0)
                    {
                        if(TryToOut(itemAmount.Key, out _))
                            return true;
                    }
                    break;
                }
                i++;
            }
            return false;
        }

        public bool TryToOut(string id, out bool isFull)
        {
            isFull = false;
            if (_counts.TryGetValue(id, out var count) && _inventoryManager.ItemDatabase.TryGet(id, out var itemData))
            {
                var startCount = count;
                var maxStack = itemData.MaxStack;
                if(maxStack>1)
                {
                    foreach (var itemAmount in _outItems)
                    {
                        if (itemAmount.Item.Id == id)
                        {
                            if (itemAmount.Amount < maxStack)
                            {
                                var sum = itemAmount.Amount + count;
                                if(sum > maxStack)
                                {
                                    count = sum - maxStack;
                                    itemAmount.Amount = maxStack;
                                    continue;
                                }

                                itemAmount.Amount = sum;
                                break;
                            }
                        }
                    }
                }
                while (count > 0)
                {
                    if (Capacity <= 0 || _outItems.Count < Capacity)
                    {
                        var pCount = maxStack > count ? count : maxStack;
                        count -= pCount;
                        _outItems.Add(new ItemAmount()
                        {
                            Item = itemData,
                            Amount = pCount
                        });
                    }
                    else
                    {
                        isFull = true;
                        break;
                    }
                }
                _counts[id] = count;
                
                return startCount != count;
            }

            return false;
        }
        
        public bool RemoveFromOut(int index)
        {
            if (index >= 0 && index < _outItems.Count)
            {
                var count = _outItems[index].Amount;
                _counts[(_outItems[index].Item.Id)] += count;
                _outItems.RemoveAt(index);
                return true;
            }
            return false;
        }

        public List<ItemAmount> GetOutItems()
        {
            return _outItems;
        }

        public List<ItemAmount> GetLostItems()
        {
            var lostItems = new List<ItemAmount>();
            foreach (var itemAmount in _counts)
            {
                if(itemAmount.Value>0 && _inventoryManager.ItemDatabase.TryGet(itemAmount.Key, out var itemData))
                {
                    lostItems.Add(new ItemAmount() { Item = itemData, Amount = itemAmount.Value });
                }
            }
            return lostItems;
        }

        public void TakeOut()
        {
            if(_outItems.Count>0)
            {
                _inventoryManager.Add(_outItems);
            }
            Clear();
        }

        public void TakeAll()
        {
            foreach (var itemAmount in _counts)
            {
                if (itemAmount.Value > 0 && _inventoryManager.ItemDatabase.TryGet(itemAmount.Key, out var itemData))
                    _outItems.Add( new ItemAmount()
                    {
                        Item = itemData,
                        Amount = itemAmount.Value
                    });
            }
            TakeOut();
        }

        public void AddCapacity(int capacity = 1)
        {
            Capacity += capacity;
        }
        
        public void Clear()
        {
            _counts.Clear();
            _outItems.Clear();
        }
    }
}