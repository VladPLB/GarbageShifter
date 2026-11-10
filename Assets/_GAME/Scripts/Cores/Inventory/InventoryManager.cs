using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Save;
using UnityEngine;
using Random = System.Random;

namespace _GAME.Scripts.Inventory
{
    public enum ItemRank { Common, Uncommon, Rare, Epic, Legendary, Unique }
    // New values must be appended at the end - Unity serializes enum fields on ItemInfo assets by index,
    // so inserting/reordering here would silently reassign the type of already-authored items.
    public enum ItemType { Currency, Equipment, Consumable, Material, Quest, Other, Weapon, SecondaryWeapon, License, Chest, Offer }

    public enum PlaceTarget
    {
        None = -1,
        Inventory = 0,
        LocalInventory = 1,
        Shop = 10,
        
        Store = 20,
        
        Upgrade = 30,
    }

    public enum InventoryCase
    {
        Weapon,
        SubWeapon,
        Armor,
        Drone,
        Crew,
        Ship
    }

    public enum UpgradeResult
    {
        Available = 0,
        NotItem = 1,
        NotEnoughItems = 2,
        MaxLevel = 3,
        NoUpgradable = 34,
        
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
    public class ItemAmountGroup
    {
        public List<ItemAmount> Items = new();
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
        private readonly Dictionary<string, int> _upgrades = new();

        private readonly Dictionary<InventoryCase, string> _cases = new();
        private PlaceTarget _placeTarget = PlaceTarget.None;
        
        public ItemDatabase ItemDatabase => _dataBase.ItemDatabase;

        public PlaceTarget PlaceTarget
        {
            get => _placeTarget;
            set
            {
                if (_placeTarget == value)
                    return;

                _placeTarget = value;
                EventBus.Push(new PlaceTargetChangedEvent(value), EventBus.EventRegion.GLOBAL);
            }
        }
        
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
            return ignoreShowConditions ? allItems : allItems.Where(CanShow).ToList();
        }
        
        public List<ItemInfo> GetAll(ItemType type, bool ignoreShowConditions = false)
        {
            var allItems = ItemDatabase.GetByType(type);
            return ignoreShowConditions ? allItems : allItems.Where(CanShow).ToList();
        }
        
        public List<ItemInfo> GetAll(ItemType type, string subType, bool ignoreShowConditions = false)
        {
            var allItems = string.IsNullOrEmpty(subType)? ItemDatabase.GetByType(type): ItemDatabase.GetByTypeSubType(type, subType);
            return ignoreShowConditions ? allItems : allItems.Where(CanShow).ToList();
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
            return Add(item, amount, null);
        }

        // received: what actually ended up in the inventory (an already owned equipment item is converted to currency)
        public bool Add(ItemInfo item, int amount, List<ItemAmount> received)
        {
            if (item == null) return false;
            if (amount == 0) return false;
            if (string.IsNullOrWhiteSpace(item.Id)) return false;

            var current = GetCount(item);
            bool isUpgradable = (item is UpgradableItemInfo);

            if (isUpgradable && current > 0)
            {
                return amount > 0 && AddDuplicateCompensation(item, amount, received);
            }
            
            var target = current + amount;
            
            if (target < 0) return false;

            if (target == 0)
            {
                _counts.Remove(item.Id);
                _upgrades.Remove(item.Id);
            }
            else
            {
                _counts[item.Id] = target;
                if (isUpgradable)
                {
                    _upgrades[item.Id] = 0;
                }
            }
            
            WriteCacheToSave();
            EventBus.Push(new InventoryItemChangedEvent(item, current, target), EventBus.EventRegion.GLOBAL);
            if (amount > 0)
            {
                received?.Add(new ItemAmount { Item = item, Amount = amount });
            }
            return true;
        }

        // Duplicate equipment is paid out with the currency part of its recipe price
        private bool AddDuplicateCompensation(ItemInfo item, int amount, List<ItemAmount> received)
        {
            var costs = item.Recipe?.ItemCosts;
            if (costs == null) return false;

            var result = false;
            foreach (var cost in costs)
            {
                if (cost.Item == null || cost.Item.Type != ItemType.Currency || cost.Amount <= 0)
                    continue;

                if (Add(cost.Item, cost.Amount * amount, received))
                    result = true;
            }
            return result;
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

        public int GetLevel(ItemInfo item)
        {
            return _upgrades.GetValueOrDefault(item.Id, 0);
        }

        public bool TryCanUpgrade(ItemInfo info, out UpgradeResult result, out List<ItemAmount> missingIngredients)
        {
            missingIngredients = new List<ItemAmount>();
            result = UpgradeResult.Available;
            if (GetCount(info) != 1)
            {
                result = UpgradeResult.NotItem;
                return false;
            }
            
            if(!_upgrades.TryGetValue(info.Id, out var upgrade))
            {
                result = UpgradeResult.NoUpgradable;
                return false;
            }

            if (info is not UpgradableItemInfo upgradeInfo)
            {
                result = UpgradeResult.NoUpgradable;
                return false;
            }

            if (upgrade >= upgradeInfo.MaxLevel)
            {
                result = UpgradeResult.MaxLevel;
                return false;
            }

            var price = upgradeInfo.GetPriceByLevel(upgrade);
            bool hasEnoughItems = true;
            for (int i = 0; i < price.Count; i++)
            {
                var cost = price[i];
                if (cost.Item == null) return false;
                if (cost.Amount <= 0) return false;

                var have = GetCount(cost.Item);
                if (have < cost.Amount)
                {
                    hasEnoughItems = false;
                    missingIngredients.Add(new ItemAmount
                    {
                        Item = cost.Item,
                        Amount = cost.Amount - have
                    });
                }
            }
            
            result = hasEnoughItems?  result : UpgradeResult.NotEnoughItems;
            return hasEnoughItems;
        }

        public bool TryUpgrade(ItemInfo info, out UpgradeResult result, out List<ItemAmount> missingIngredients)
        {
            if (!TryCanUpgrade(info, out result, out missingIngredients))
            {
                return false;
            }

            var upgrade = _upgrades[info.Id];
            Remove(((UpgradableItemInfo)info).GetPriceByLevel(upgrade));
            _upgrades[info.Id] = upgrade + 1;
            
            WriteCacheToSave();
            AnalyticsEvent.Push("equipment_upgrade",
                ("item_id", info.Id),
                ("item_type", info.Type.ToString()),
                ("rank", info.Rank.ToString()),
                ("item_level", upgrade + 2),
                ("is_equipped", CanEquip(info)));
            
            return true;
        }
        
        public bool CanCraftByRecipe(ItemInfo craftTarget, out List<ItemAmount> missingIngredients)
        {
            missingIngredients = new List<ItemAmount>();

            if (craftTarget == null) return false;
            if (craftTarget.Recipe == null) return false;
            if(!CanCraft(craftTarget)) return false;

            var recipe = craftTarget.Recipe;

            if (recipe.ItemCosts == null || recipe.ItemCosts.Count == 0)
                return false;
            
            if (craftTarget is UpgradableItemInfo && GetCount(craftTarget.Id) > 1) return false;

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
        
        public bool TryCraftByRecipe(ItemInfo craftTarget, out List<ItemAmount> results, System.Random rng = null)
        {
            results = new List<ItemAmount>();
            if (craftTarget == null) return false;
            if (craftTarget.Recipe == null) return false;
            

            if (!HasLoot(craftTarget.Recipe.Loot))
                return false;

            if (!CanCraftByRecipe(craftTarget))
                return false;

            if (!ConsumeRecipeIngredients(craftTarget))
                return false;

            results = UnpackLootTable(craftTarget.Recipe.Loot, rng);

            return true;
        }

        private static bool HasLoot(LootTable loot)
        {
            return loot != null && loot.Entries != null && loot.Entries.Count > 0 && loot.Rolls > 0;
        }

        public List<ItemAmount> UnpackLootTable(LootTable loot, Random rng = null)
        {
            if (!HasLoot(loot))
            {
                return new List<ItemAmount>();
            }
            
            List<ItemAmount>  results = new List<ItemAmount>();

            if (loot.Entries.Count == 1)
            {
                var e = loot.Entries[0];
                var amount = e.RollAmount(rng);
                if (amount > 0)
                {
                    Add(e.Item, amount, results);
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
                            Add(e.Item, amount, results);
                            break;
                        }
                    }
                }
            }

            return results;
        }

        public bool CanEquip(ItemInfo itemInfo)
        {
            foreach (var i in _cases)
            {
               if(i.Value == itemInfo.Id) return true;
            }
            return false;
        }

        // Daily rewards are bound to the local calendar day, not to the time of the last claim
        public static int CurrentDay => (int)(DateTime.Now.Date - new DateTime(2000, 1, 1)).TotalDays;
        public static TimeSpan TimeToNextDay => DateTime.Now.Date.AddDays(1) - DateTime.Now;

        public bool IsDailyClaimAvailable(string id)
        {
            if (_inventoryData?.DailyClaims == null)
                return true;

            foreach (var claim in _inventoryData.DailyClaims)
            {
                if (claim.Id == id)
                    return claim.Day != CurrentDay;
            }
            return true;
        }

        public void MarkDailyClaimed(string id)
        {
            if (_inventoryData == null)
                return;

            _inventoryData.DailyClaims ??= new List<DailyClaimSave>();
            _inventoryData.DailyClaims.RemoveAll(c => c.Id == id);
            _inventoryData.DailyClaims.Add(new DailyClaimSave { Id = id, Day = CurrentDay });
            _inventoryData.MarkDirty(forced: true);
        }

        public bool TryGetEquipped(InventoryCase inventoryCase, out ItemInfo item, out int level)
        {
            item = null;
            level = 0;
            if (!_cases.TryGetValue(inventoryCase, out var id) || string.IsNullOrEmpty(id))
                return false;

            if (!ItemDatabase.TryGet(id, out item) || GetCount(item) <= 0)
                return false;

            level = GetLevel(item);
            return true;
        }

        public bool TryEquip(ItemInfo itemInfo)
        {
            var data = _dataBase.GetEquipmentData(itemInfo.Id);
            if(data == null) return false;
            
            _cases.TryGetValue(data.InventoryCase, out var previousId);
            _cases[data.InventoryCase] = itemInfo.Id;
            WriteCacheToSave();
            AnalyticsEvent.Push("equip_change",
                ("slot", data.InventoryCase.ToString()),
                ("item_id", itemInfo.Id),
                ("prev_item_id", previousId ?? string.Empty),
                ("rank", itemInfo.Rank.ToString()),
                ("item_level", GetLevel(itemInfo) + 1));
            
            return true;
        }

        private void RebuildCacheFromSave()
        {
            _counts.Clear();
            _upgrades.Clear();
            _cases.Clear();

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

            // owned upgradable items start at level 0 even without a saved upgrade entry (e.g. default save)
            foreach (var id in _counts.Keys)
            {
                if (ItemDatabase.TryGet(id, out var info) && info is UpgradableItemInfo)
                    _upgrades[id] = 0;
            }

            if (_inventoryData.Upgrades != null)
            {
                foreach (var upgrade in _inventoryData.Upgrades)
                {
                    if (_counts.ContainsKey(upgrade.ItemId))
                        _upgrades[upgrade.ItemId] = upgrade.Level;
                }
            }

            if (_inventoryData.Cases != null)
            {
                foreach (var equipped in _inventoryData.Cases)
                {
                    if (_counts.ContainsKey(equipped.ItemId))
                        _cases[equipped.Case] = equipped.ItemId;
                }
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
            
            _inventoryData.Upgrades.Clear();
            foreach (var kv in _upgrades)
            {
                _inventoryData.Upgrades.Add(new ItemUpgradeSave
                {
                    ItemId = kv.Key,
                    Level = kv.Value
                });
            }
            
            _inventoryData.Cases.Clear();
            foreach (var kv in _cases)
            {
                if(kv.Value!=string.Empty)
                {
                    _inventoryData.Cases.Add(new ItemCaseSave
                    {
                        ItemId = kv.Value,
                        Case = kv.Key
                    });
                }
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
                if (!c.CanUse())
                    return false;
            }
            return true;
        }
        
        public List<ItemUsePipelineSO> GetUseActions(ItemInfo item)
        {
            var actions = new List<ItemUsePipelineSO>();
            if (item == null || !CanUse(item))
                return actions;

            foreach (var pipeline in item.UsePipelines)
            {
                if (pipeline != null)
                    pipeline.CollectActions(item, actions);
            }
            return actions;
        }

        public List<T> GetUseInfos<T>(ItemInfo item) where T : ItemUseInfo
        {
            var infos = new List<ItemUseInfo>();
            if (item == null)
                return new List<T>();

            foreach (var pipeline in item.UsePipelines)
            {
                if (pipeline != null)
                    pipeline.CollectUseInfos(item, infos);
            }
            return infos.OfType<T>().ToList();
        }

        public bool CanExecuteUse(ItemInfo item, ItemUsePipelineSO action)
        {
            return action != null && GetUseActions(item).Contains(action) && action.CanExecute(item);
        }

        public bool Use(ItemInfo item, ItemUsePipelineSO action, Action<ItemUseResult> callback = null)
        {
            var result = new ItemUseResult(item, action);
            result.Success = CanExecuteUse(item, action) && action.Use(item, result);
            callback?.Invoke(result);
            return result.Success;
        }

        public bool Use(ItemInfo item, Action<ItemUseResult> callback = null)
        {
            var actions = GetUseActions(item);
            return Use(item, actions.Count > 0 ? actions[0] : null, callback);
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