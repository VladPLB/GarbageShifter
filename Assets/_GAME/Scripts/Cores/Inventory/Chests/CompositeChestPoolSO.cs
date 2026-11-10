using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Inventory.Chests
{
    // Every reward picks one of the sub pools by weight (pools that can't give anything are skipped)
    [CreateAssetMenu(menuName = "Inventory/Chests/CompositePool", fileName = "ChestPool_Mixed_")]
    public class CompositeChestPoolSO : ChestRewardPoolSO
    {
        [Serializable]
        private class WeightedPool
        {
            public ChestRewardPoolSO Pool;
            [Min(0f)] public float Weight = 1f;
        }

        [SerializeField] private List<WeightedPool> _pools = new();

        public override int GetAvailableCount(InventoryManager inventory)
        {
            var count = 0L;
            foreach (var entry in _pools.Where(p => p.Pool != null && p.Pool != this && p.Weight > 0f))
                count += entry.Pool.GetAvailableCount(inventory);
            return (int)Math.Min(count, int.MaxValue);
        }

        public override void CollectPreview(InventoryManager inventory, float chanceMultiplier, List<LootEntry> entries)
        {
            var pools = _pools.Where(p => p.Pool != null && p.Pool != this && p.Weight > 0f && p.Pool.GetAvailableCount(inventory) > 0).ToList();
            var total = pools.Sum(p => p.Weight);
            foreach (var entry in pools)
                entry.Pool.CollectPreview(inventory, chanceMultiplier * entry.Weight / total, entries);
        }

        public override bool TryRoll(InventoryManager inventory, ISet<string> excludedIds, out ItemAmount reward)
        {
            var candidates = _pools.Where(p => p.Pool != null && p.Pool != this && p.Weight > 0f).ToList();
            while (candidates.Count > 0)
            {
                var entry = PickWeighted(candidates);
                if (entry.Pool.TryRoll(inventory, excludedIds, out reward))
                    return true;

                candidates.Remove(entry);
            }

            reward = null;
            return false;
        }

        private static WeightedPool PickWeighted(List<WeightedPool> pools)
        {
            var roll = Random.value * pools.Sum(p => p.Weight);
            foreach (var pool in pools)
            {
                roll -= pool.Weight;
                if (roll <= 0f)
                    return pool;
            }
            return pools[pools.Count - 1];
        }
    }
}
