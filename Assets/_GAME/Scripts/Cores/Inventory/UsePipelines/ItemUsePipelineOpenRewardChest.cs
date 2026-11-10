using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory.Chests;
using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    // Opens a chest (spends the chest item) with rewards rolled from a reward pool
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/OpenRewardChest", fileName = "ItemUsePipelineOpenRewardChest_")]
    public class ItemUsePipelineOpenRewardChest : ItemUsePipelineSO
    {
        [SerializeField] protected int _spendAmount = 1;
        [SerializeField, Min(1)] protected int _rewardsCount = 1;
        [SerializeField] protected ChestRewardPoolSO _pool;
        [Tooltip("Places where the chest can be opened")]
        [SerializeField] protected List<PlaceTarget> _places = new() { PlaceTarget.Inventory };

        public ChestRewardPoolSO Pool => _pool;
        public int RewardsCount => _rewardsCount;

        // Chest content with chances for the item info window (same preview as recipe loot)
        public override void CollectUseInfos(ItemInfo info, List<ItemUseInfo> infos)
        {
            base.CollectUseInfos(info, infos);
            if (_pool == null)
                return;

            var loot = new LootTable { Rolls = _rewardsCount, Entries = _pool.GetPreview(Core.Get<InventoryManager>()) };
            infos.Add(new ItemUseLootInfo(loot));
        }

        public override bool IsAvailable(ItemInfo info)
        {
            return _places.Contains(Core.Get<InventoryManager>().PlaceTarget);
        }

        public override bool CanExecute(ItemInfo info)
        {
            return _pool != null && CanPay(info, Core.Get<InventoryManager>());
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            var inventoryManager = Core.Get<InventoryManager>();
            if (_pool == null || !CanPay(info, inventoryManager) || !Pay(info, inventoryManager))
                return false;

            foreach (var reward in _pool.Roll(inventoryManager, _rewardsCount))
            {
                inventoryManager.Add(reward.Item, reward.Amount, result.ReceivedLoot);
            }

            var price = info.Recipe.ItemCosts.Count > 0 ? info.Recipe.ItemCosts[0] : null;
            AnalyticsEvent.Push("chest_open",
                ("chest_id", info.Id),
                ("source", Source),
                ("place", inventoryManager.PlaceTarget.ToString()),
                ("currency", price?.Item != null && Source == "buy" ? price.Item.Id : null),
                ("price", Source == "buy" ? price?.Amount ?? 0 : 0),
                ("rewards_count", result.ReceivedLoot.Count),
                ("rewards", string.Join(",", result.ReceivedLoot.Select(r => $"{r.Item.Id}x{r.Amount}"))));
            return true;
        }

        // how the chest was paid, for analytics
        protected virtual string Source => "item";

        protected virtual bool CanPay(ItemInfo info, InventoryManager inventory) => inventory.GetCount(info) >= _spendAmount;
        protected virtual bool Pay(ItemInfo info, InventoryManager inventory) => inventory.Remove(info, _spendAmount);
    }
}
