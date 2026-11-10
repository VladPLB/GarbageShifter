using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Chests
{
    // Random equipment the player doesn't own yet, every item at most once
    [CreateAssetMenu(menuName = "Inventory/Chests/EquipmentPool", fileName = "ChestPool_Equipment_")]
    public class EquipmentChestPoolSO : ChestRewardPoolSO
    {
        [SerializeField] private List<ItemType> _itemTypes = new() { ItemType.Weapon, ItemType.SecondaryWeapon };

        public override int GetAvailableCount(InventoryManager inventory) => GetCandidates(inventory, null).Count;

        public override bool TryRoll(InventoryManager inventory, ISet<string> excludedIds, out ItemAmount reward)
        {
            reward = null;
            var candidates = GetCandidates(inventory, excludedIds);
            if (!TryRollRank(rank => candidates.Any(i => i.Rank == rank), out var rolledRank))
                return false;

            var items = candidates.Where(i => i.Rank == rolledRank).ToList();
            reward = new ItemAmount { Item = items[Random.Range(0, items.Count)], Amount = 1 };
            return true;
        }

        public override void CollectPreview(InventoryManager inventory, float chanceMultiplier, List<LootEntry> entries)
        {
            var candidates = GetCandidates(inventory, null);
            foreach (var rankChance in GetRankChances(rank => candidates.Any(i => i.Rank == rank)))
            {
                var items = candidates.Where(i => i.Rank == rankChance.Key).ToList();
                foreach (var item in items)
                    entries.Add(CreatePreviewEntry(item, 1, chanceMultiplier * rankChance.Value / items.Count));
            }
        }

        private List<ItemInfo> GetCandidates(InventoryManager inventory, ISet<string> excludedIds)
        {
            return _itemTypes
                .SelectMany(type => inventory.GetAll(type, true))
                .Where(item => item is UpgradableItemInfo
                               && item.Rank <= _maxRank
                               && inventory.GetCount(item) <= 0
                               && (excludedIds == null || !excludedIds.Contains(item.Id)))
                .ToList();
        }
    }
}
