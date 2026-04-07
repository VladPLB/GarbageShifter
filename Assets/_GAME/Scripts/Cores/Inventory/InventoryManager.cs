using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Save;
using UnityEngine;
using Random = System.Random;

namespace _GAME.Scripts.Inventory
{
    public enum ItemRank { Common, Uncommon, Rare, Epic, Legendary, Unique }
    public enum ItemType { Currency, Equipment, Consumable, Material, Quest, Other }

    public enum PlaceTarget
    {
        None = -1,
        Inventory = 0,
        LocalInventory = 1,
        Shop = 10,
        Store = 20,
    }
    
    [Serializable]
    public struct LootEntry
    {
        public ItemInfo Item;
        [Min(1)] public int MinAmount;
        [Min(1)] public int MaxAmount;
        public int Group;

        [Tooltip("Шанс выпадения конкретного entry в процентах [0..100].")]
        [Range(0f, 100f)] public float ChancePercent;

        public int RollAmount(System.Random rng)
        {
            var min = Mathf.Max(1, MinAmount);
            var max = Mathf.Max(min, MaxAmount);
            
            if(min == max)
                return max;
            
            return rng == null ? UnityEngine.Random.Range(min, max + 1) : rng.Next(min, max + 1);
        }

        public bool RollSuccess(System.Random rng)
        {
            var roll = rng == null ? UnityEngine.Random.value * 100f : (float)rng.NextDouble() * 100f;
            return roll <= ChancePercent;
        }
    }

    [Serializable]
    public class LootTable
    {
        [Min(0)] public int Rolls = 1;
        public List<LootEntry> Entries = new();
    }
    
    [Serializable]
    public class ItemAmount
    {
        public ItemInfo Item;
        [Min(0)] public int Amount;
    }

    [Serializable]
    public class ItemRecipe
    {
        [Header("Ingredients (required)")]
        public List<ItemAmount> ItemCosts = new();

        [Header("Result (loot table)")]
        public LootTable Loot = new();
    }

    
    public class InventoryManager: MonoBehaviour, IRuntimeSetup
    {
        private DataBase _dataBase;
        private SaveManager _saveManager;
        private InventoryData _inventoryData;

        private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);
        
        public ItemDatabase ItemDatabase => _dataBase.ItemDatabase;
        public PlaceTarget PlaceTarget { get; set; } = PlaceTarget.None;
        
        private void Awake()
        {
            Core.Registry(this, typeof(DataBase), typeof(SaveManager));
        }
        public void RuntimeSetup()
        {
            _dataBase = Core.Get<DataBase>();
            _saveManager = Core.Get<SaveManager>();
            _inventoryData = _saveManager.GetData<InventoryData>();
            RebuildCacheFromSave();
        }

        public Dictionary<ItemInfo, int> GetMyInventory(ItemType itemType)
        {
            var dict = new Dictionary<ItemInfo, int>();
            foreach (var item in _counts)
            {
                if(ItemDatabase.TryGet(item.Key, out var itemInfo))
                {
                    if(itemInfo.Type == itemType && CanShow(itemInfo))
                    {
                        dict.Add(itemInfo, item.Value);
                    }
                }
            }
            return dict;
        }

        public List<ItemInfo> GetAll(bool ignoreShowConditions = false)
        {
            var allItems = ItemDatabase.GetAll();
            return !ignoreShowConditions? allItems : allItems.Where(CanShow).ToList();
        }
        
        public List<ItemInfo> GetAll(ItemType type, bool ignoreShowConditions = false)
        {
            var allItems = ItemDatabase.GetByType(type);
            return !ignoreShowConditions? allItems : allItems.Where(CanShow).ToList();
        }
        
        public List<ItemInfo> GetAll(ItemType type, string subType = "", bool ignoreShowConditions = false)
        {
            var allItems = string.IsNullOrEmpty(subType)? ItemDatabase.GetByType(type): ItemDatabase.GetByTypeSubType(type, subType);
            return !ignoreShowConditions? allItems : allItems.Where(CanShow).ToList();
        }

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
            
            WriteCacheToSave();
            return true;
        }

        public bool Add(string id, int amount)
        {
            if (amount != 0 && ItemDatabase.TryGet(id, out var item))
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
        
        public bool CanCraftByRecipe(ItemInfo craftTarget, out List<ItemAmount> missingIngredients)
        {
            missingIngredients = new List<ItemAmount>();

            if (craftTarget == null) return false;
            if (craftTarget.Recipe == null) return false;

            var recipe = craftTarget.Recipe;

            if (recipe.ItemCosts == null || recipe.ItemCosts.Count == 0)
                return false;

            var canCraft = true;

            for (int i = 0; i < recipe.ItemCosts.Count; i++)
            {
                var cost = recipe.ItemCosts[i];
                if (cost.Item == null) return false;
                if (cost.Amount <= 0) return false;

                var have = GetCount(cost.Item);
                if (have < cost.Amount)
                {
                    canCraft = false;
                    missingIngredients.Add(new ItemAmount
                    {
                        Item = cost.Item,
                        Amount = cost.Amount - have
                    });
                }
            }

            return canCraft;
        }
        
        public bool CanCraftByRecipe(ItemInfo craftTarget)
        {
            return CanCraftByRecipe(craftTarget, out _);
        }
        
        public bool ConsumeRecipeIngredients(ItemInfo craftTarget)
        {
            if (craftTarget == null) return false;
            if (craftTarget.Recipe == null) return false;

            var recipe = craftTarget.Recipe;

            if (recipe.ItemCosts == null || recipe.ItemCosts.Count == 0)
                return false;

            // safety: повторная проверка, чтобы не уйти в минус
            for (int i = 0; i < recipe.ItemCosts.Count; i++)
            {
                var cost = recipe.ItemCosts[i];
                if (cost.Item == null) return false;
                if (GetCount(cost.Item) < cost.Amount) return false;
            }

            for (int i = 0; i < recipe.ItemCosts.Count; i++)
            {
                var cost = recipe.ItemCosts[i];
                Remove(cost.Item, cost.Amount);
            }

            return true;
        }
        
        public List<ItemAmount> CraftByRecipe(ItemInfo craftTarget, System.Random rng = null)
        {
            List<ItemAmount> results = null;
            if (craftTarget == null) return results;
            if (craftTarget.Recipe == null) return results;

            if (!CanCraftByRecipe(craftTarget))
                return results;

            if (!ConsumeRecipeIngredients(craftTarget))
                return results;

            results = UnpackLootTable(craftTarget.Recipe.Loot, rng);

            return results;
        }

        public List<ItemAmount> UnpackLootTable(LootTable loot, Random rng = null)
        {
            if (loot == null || loot.Entries == null || loot.Entries.Count == 0 || loot.Rolls <= 0)
            {
                return null;
            }
            
            List<ItemAmount>  results = new List<ItemAmount>();

            if (loot.Entries.Count == 1)
            {
                var e = loot.Entries[0];
                var amount = e.RollAmount(rng);
                if (amount > 0)
                {
                    Add(e.Item, amount);
                    results.Add(new ItemAmount()
                    {
                        Item = e.Item,
                        Amount = amount
                    });
                }

                return results;
            }
            
            Dictionary<int, List<LootEntry>> EntriesByGroup = new Dictionary<int, List<LootEntry>>();
            for (int i = 0; i < loot.Entries.Count; i++)
            {
                var e = loot.Entries[i];
                if (e.Item == null) continue;

                if (!EntriesByGroup.ContainsKey(e.Group))
                {
                    EntriesByGroup.Add(e.Group, new List<LootEntry>());
                }
                EntriesByGroup[e.Group].Add(e);
                break;
            }
            

            for (int r = 0; r < loot.Rolls; r++)
            {
                foreach (var group in EntriesByGroup.Values)
                {
                    for (int i = 0; i < group.Count; i++)
                    {
                        var e = group[i];
                        if (i< group.Count-1 && !e.RollSuccess(rng)) continue;

                        var amount = e.RollAmount(rng);
                        if (amount > 0)
                        {
                            Add(e.Item, amount);
                            results.Add(new ItemAmount()
                            {
                                Item = e.Item,
                                Amount = amount
                            });
                            break;
                        }
                    }
                }
            }

            return results;
        }

        private void RebuildCacheFromSave()
        {
            _counts.Clear();

            if (_inventoryData?.Items == null)
                return;

            foreach (var stack in _inventoryData.Items)
            {
                if (string.IsNullOrWhiteSpace(stack.ItemId)) 
                    continue;
                
                if (!ItemDatabase.TryGet(stack.ItemId, out var info))
                    continue;

                if (stack.Amount <= 0 && !info.ZeroCountEnabled) 
                    continue;
                
                _counts[stack.ItemId] = stack.Amount;
            }
        }

        private void WriteCacheToSave()
        {
            if (_inventoryData == null) return;

            _inventoryData.Items.Clear();
            foreach (var kv in _counts)
            {
                _inventoryData.Items.Add(new ItemStackSave
                {
                    ItemId = kv.Key,
                    Amount = kv.Value
                });
            }

            _inventoryData.MarkDirty(forced: false);
        }


        public bool CanShow(string id)
        {
            if (ItemDatabase.TryGet(id, out var item))
            {
                return CanShow(item);
            }
            return false;
        }
        
        public bool CanShow(ItemInfo item)
        {
            if (item == null) return false;
            var list = item.ShowConditions;
            if (list == null || list.Count == 0) return true;

            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c == null) continue;
                if (!c.CanShow()) return false;
            }
            return true;
        }

        public bool CanCraft(ItemInfo item)
        {
            if (item == null) return false;
            if(item.Recipe == null) return false;
            if(item.Recipe.ItemCosts == null || item.Recipe.ItemCosts.Count == 0) return false;
            
            var list = item.CraftConditions;
            if (list == null || list.Count == 0) return true;

            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c == null) continue;
                if (!c.CanCraft()) return false;
            }
            return true;
        }
        
        public bool CanCraft(string id)
        {
            if (ItemDatabase.TryGet(id, out var item))
            {
                return CanCraft(item);
            }
            return false;
        }

        public bool CanUse(ItemInfo item)
        {
            if (item == null) return false;
            var list = item.UseConditions;
            if (list == null || list.Count == 0) return true;

            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c == null) continue;
                if (!c.CanUse()) return false;
            }
            return true;
        }
        
        public bool CanUse(string id)
        {
            if (ItemDatabase.TryGet(id, out var item))
            {
                return CanUse(item);
            }
            return false;
        }

    }
}