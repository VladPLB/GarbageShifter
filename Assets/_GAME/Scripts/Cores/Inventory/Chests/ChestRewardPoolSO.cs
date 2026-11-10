using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Chests
{
    // Strategy of chest rewards: what can drop and how the rank is rolled
    public abstract class ChestRewardPoolSO : ScriptableObject
    {
        [Header("Rank")]
        [Tooltip("Weight of rank r = falloff^r: lower - high ranks are rarer (bigger chests use bigger values)")]
        [SerializeField, Range(0.05f, 1f)] protected float _rankFalloff = 0.4f;
        [SerializeField] protected ItemRank _maxRank = ItemRank.Unique;

        // excludedIds: rewards already picked in this chest (unique rewards must not repeat)
        public abstract bool TryRoll(InventoryManager inventory, ISet<string> excludedIds, out ItemAmount reward);

        // Possible rewards of a single roll with their chance (0..100) for the item info preview
        public abstract void CollectPreview(InventoryManager inventory, float chanceMultiplier, List<LootEntry> entries);

        public List<LootEntry> GetPreview(InventoryManager inventory)
        {
            var entries = new List<LootEntry>();
            CollectPreview(inventory, 1f, entries);
            return entries;
        }

        // How many different rewards the pool can give right now (unique pools are limited)
        public virtual int GetAvailableCount(InventoryManager inventory) => int.MaxValue;

        public List<ItemAmount> Roll(InventoryManager inventory, int count)
        {
            var rewards = new List<ItemAmount>();
            var excluded = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                if (TryRoll(inventory, excluded, out var reward))
                {
                    rewards.Add(reward);
                    excluded.Add(reward.Item.Id);
                }
            }
            return rewards;
        }

        // Rolls a rank among the ones accepted by the filter; false if none is accepted
        protected bool TryRollRank(System.Func<ItemRank, bool> hasRewards, out ItemRank rank)
        {
            var total = 0f;
            for (int r = 0; r <= (int)_maxRank; r++)
            {
                if (hasRewards((ItemRank)r))
                    total += GetRankWeight(r);
            }

            rank = ItemRank.Common;
            if (total <= 0f)
                return false;

            var roll = Random.value * total;
            for (int r = 0; r <= (int)_maxRank; r++)
            {
                if (!hasRewards((ItemRank)r))
                    continue;

                rank = (ItemRank)r;
                roll -= GetRankWeight(r);
                if (roll <= 0f)
                    break;
            }
            return true;
        }

        // Chance (0..1) of every rank accepted by the filter
        protected Dictionary<ItemRank, float> GetRankChances(System.Func<ItemRank, bool> hasRewards)
        {
            var chances = new Dictionary<ItemRank, float>();
            var total = 0f;
            for (int r = 0; r <= (int)_maxRank; r++)
            {
                if (hasRewards((ItemRank)r))
                    total += GetRankWeight(r);
            }
            if (total <= 0f)
                return chances;

            for (int r = 0; r <= (int)_maxRank; r++)
            {
                if (hasRewards((ItemRank)r))
                    chances[(ItemRank)r] = GetRankWeight(r) / total;
            }
            return chances;
        }

        protected static LootEntry CreatePreviewEntry(ItemInfo item, int amount, float chance)
        {
            return new LootEntry
            {
                Item = item,
                MinAmount = amount,
                MaxAmount = amount,
                ChancePercent = chance * 100f,
            };
        }

        private float GetRankWeight(int rank) => Mathf.Pow(_rankFalloff, rank);
    }
}
