using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Ads;
using _GAME.Scripts.Purchasing;
using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    // Real money offer: starts the store purchase, rewards are granted later by PurchaseManager (PurchaseCompletedEvent)
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/IapPurchase", fileName = "ItemUsePipelineIapPurchase_")]
    public class ItemUsePipelineIapPurchase : ItemUsePipelineSO
    {
        [Tooltip("Product id from the Google Play Console")]
        [SerializeField] private string _productId;
        [Tooltip("Consumable can be bought many times; non consumable once (e.g. NoAds), restored by the store")]
        [SerializeField] private bool _isConsumable = true;
        [SerializeField] private List<ItemAmount> _rewards = new();

        public string ProductId => _productId;
        public bool IsConsumable => _isConsumable;
        public IReadOnlyList<ItemAmount> Rewards => _rewards;

        public string LocalizedPrice => Core.Get<PurchaseManager>()?.GetLocalizedPrice(_productId);
        public decimal? Price => Core.Get<PurchaseManager>()?.GetPrice(_productId);
        public bool GivesNoAds => _rewards.Exists(r => r.Item != null && r.Item.Id == NoAds.ItemId);

        public override ItemUseButtonView GetButtonView(ItemInfo info)
        {
            var price = LocalizedPrice;
            return string.IsNullOrEmpty(price) ? base.GetButtonView(info) : new ItemUseButtonView(price);
        }

        public override bool CanExecute(ItemInfo info)
        {
            return !string.IsNullOrEmpty(LocalizedPrice);
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            var purchaseManager = Core.Get<PurchaseManager>();
            return purchaseManager != null && purchaseManager.Purchase(_productId);
        }

        public override void CollectUseInfos(ItemInfo info, List<ItemUseInfo> infos)
        {
            base.CollectUseInfos(info, infos);
            var entries = _rewards
                .Where(r => r.Item != null)
                .Select(r => new LootEntry { Item = r.Item, MinAmount = r.Amount, MaxAmount = r.Amount, ChancePercent = 100f })
                .ToList();
            infos.Add(new ItemUseLootInfo(new LootTable { Rolls = 1, Entries = entries }));
        }
    }
}
