using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Chests
{
    // One stack of a ranked stackable item (Licenses, Tools...), rewards may repeat
    [CreateAssetMenu(menuName = "Inventory/Chests/RankedItemPool", fileName = "ChestPool_")]
    public class RankedItemChestPoolSO : ChestRewardPoolSO
    {
        [SerializeField] private ItemType _itemType = ItemType.License;
        [Tooltip("Empty - any sub type")]
        [SerializeField] private string _subType;
        [Tooltip("Amount per reward by rank (Common..Unique); the last value is used for missing ranks")]
        [SerializeField] private List<int> _amountByRank = new() { 1 };
        [SerializeField, Min(0f)] private float _amountMultiplier = 1f;

        public override void CollectPreview(InventoryManager inventory, float chanceMultiplier, List<LootEntry> entries)
        {
            var items = GetItems(inventory);
            foreach (var rankChance in GetRankChances(rank => items.Any(i => i.Rank == rank)))
            {
                var item = items.First(i => i.Rank == rankChance.Key);
                entries.Add(CreatePreviewEntry(item, GetAmount(rankChance.Key), chanceMultiplier * rankChance.Value));
            }
        }

        public override bool TryRoll(InventoryManager inventory, ISet<string> excludedIds, out ItemAmount reward)
        {
            reward = null;
            var items = GetItems(inventory);
            if (!TryRollRank(rank => items.Any(i => i.Rank == rank), out var rolledRank))
                return false;

            var item = items.First(i => i.Rank == rolledRank);
            reward = new ItemAmount { Item = item, Amount = GetAmount(rolledRank) };
            return true;
        }

        private List<ItemInfo> GetItems(InventoryManager inventory)
        {
            return inventory.GetAll(_itemType, true)
                .Where(item => string.IsNullOrEmpty(_subType) || item.SubType == _subType)
                .ToList();
        }

        private int GetAmount(ItemRank rank)
        {
            if (_amountByRank.Count == 0)
                return 1;

            var baseAmount = _amountByRank[Mathf.Min((int)rank, _amountByRank.Count - 1)];
            return Mathf.Max(1, Mathf.RoundToInt(baseAmount * _amountMultiplier));
        }
    }
}
