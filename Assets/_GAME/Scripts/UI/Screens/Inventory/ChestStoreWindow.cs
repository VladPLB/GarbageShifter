using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Inventory.UsePipelines;
using _GAME.Scripts.UI.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    // Store of chests and offers: no tabs, a bought chest is opened at once and the loot window is shown after the opening animation;
    // real money offers show the loot window when the purchase is confirmed
    public class ChestStoreWindow : UIWindow
    {
        [SerializeField] private PlaceTarget _placeTarget = PlaceTarget.Store;
        [Tooltip("Item types shown in the store (order: NoAds, free chest, offers, the rest - see GetSortGroup)")]
        [SerializeField] private List<ItemType> _itemTypes = new() { ItemType.Chest, ItemType.Offer };
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private ChestItemViewer _itemViewerPrefab;
        [SerializeField] private GameObject _emptyStateLabel;
        [Tooltip("Delay before the received loot window (time for the opening animation)")]
        [SerializeField, Min(0f)] private float _lootWindowDelay = 1.5f;

        private InventoryManager _inventoryManager;
        private UIManager _uiManager;
        private LocalPool<ChestItemViewer> _pool;
        private readonly List<ChestItemViewer> _activeViewers = new();
        private bool _isOpeningChest;

        public override void OnOpen()
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _uiManager ??= Core.Get<UIManager>();
            _inventoryManager.PlaceTarget = _placeTarget;
            _pool ??= new LocalPool<ChestItemViewer>(_itemViewerPrefab);
            _isOpeningChest = false;
            EventBus.Subscribe<PurchaseCompletedEvent>(OnPurchaseCompleted, EventBus.EventRegion.GLOBAL);

            RefreshItems();
            base.OnOpen();
        }

        public override void OnClose()
        {
            CloseItemInfo();
            ReleaseItems();
            _inventoryManager.PlaceTarget = PlaceTarget.None;
            EventBus.Unsubscribe<PurchaseCompletedEvent>(OnPurchaseCompleted, EventBus.EventRegion.GLOBAL);
            base.OnClose();
        }

        private void RefreshItems()
        {
            ReleaseItems();

            var items = _itemTypes
                .SelectMany(type => _inventoryManager.GetAll(type))
                .Distinct()
                .OrderBy(GetSortGroup)
                .ThenBy(item => item.Rank)
                .ThenBy(item => item.DisplayName)
                .ToList();

            var bestItems = GetBestItems(items);
            foreach (var item in items)
            {
                var viewer = _pool.Pop();
                viewer.Setup(item, () => OnItemClicked(item, (RectTransform)viewer.transform));
                var offer = GetIapPipeline(item);
                if (offer != null)
                    viewer.SetPrice(offer.LocalizedPrice);
                viewer.SetBest(bestItems.Contains(item));
                viewer.OnDailyAvailable += _ => RefreshItems(); // free chest is available again (midnight)
                viewer.transform.SetParent(_itemsHolder, false);
                viewer.transform.localScale = Vector3.one;
                viewer.gameObject.SetActive(true);
                _activeViewers.Add(viewer);
            }

            if (_emptyStateLabel != null)
                _emptyStateLabel.SetActive(items.Count == 0);
        }

        private void OnItemClicked(ItemInfo itemInfo, RectTransform anchor = null)
        {
            if (_isOpeningChest)
                return;

            var offer = GetIapPipeline(itemInfo);
            var request = new ItemInfoRequest(itemInfo)
            {
                Anchor = anchor,
                StatsMode = EquipmentStatsMode.Current,
                Price = offer == null ? itemInfo.Recipe?.ItemCosts : null,
                PriceOverrideText = offer?.LocalizedPrice,
            };
            request.AddUseButtons(_inventoryManager, OnItemUsed);

            _uiManager.OpenWindow<ItemInfoWindow>().Setup(request);
        }

        private void OnItemUsed(ItemUseResult result)
        {
            if (!result.Success)
            {
                Debug.LogError($"[ChestStoreWindow] Failed to use item {result.Item.DisplayName}");
                OnItemClicked(result.Item);
                return;
            }

            _uiManager.GetWindow<ItemInfoWindow>()?.Close();
            // real money offer: rewards come with PurchaseCompletedEvent
            if (result.Action is ItemUsePipelineIapPurchase)
                return;

            ShowReceivedLootDelayed(result).Forget();
        }

        private void OnPurchaseCompleted(PurchaseCompletedEvent purchaseEvent)
        {
            if (!gameObject.activeInHierarchy || purchaseEvent.Loot.Count == 0)
                return;

            _uiManager.OpenWindow<LootReceivedWindow>()?.Setup(purchaseEvent.Loot);
        }

        // "Best" badge: NoAds and the 2nd cheapest of the other real money offers
        private static HashSet<ItemInfo> GetBestItems(List<ItemInfo> items)
        {
            var best = new HashSet<ItemInfo>(items.Where(i => GetIapPipeline(i)?.GivesNoAds ?? false));
            var offers = items.Where(i => GetSortGroup(i) == 2).ToList();
            // store prices when loaded, otherwise the display order (by rank)
            if (offers.All(i => GetIapPipeline(i).Price != null))
                offers = offers.OrderBy(i => GetIapPipeline(i).Price.Value).ToList();
            if (offers.Count > 1)
                best.Add(offers[1]);
            return best;
        }

        // 0 - NoAds offer, 1 - free daily chest, 2 - other real money offers, 3 - the rest
        private static int GetSortGroup(ItemInfo item)
        {
            var offer = GetIapPipeline(item);
            if (offer != null && offer.GivesNoAds)
                return 0;
            if (item.UsePipelines.OfType<ItemUsePipelineDailyRewardChest>().Any())
                return 1;
            if (offer != null)
                return 2;
            return 3;
        }

        private static ItemUsePipelineIapPurchase GetIapPipeline(ItemInfo item)
        {
            return item.UsePipelines.OfType<ItemUsePipelineIapPurchase>().FirstOrDefault();
        }

        private async UniTaskVoid ShowReceivedLootDelayed(ItemUseResult result)
        {
            _isOpeningChest = true;
            EventBus.Push(new ChestOpenedEvent(result.Item, result.ReceivedLoot), EventBus.EventRegion.LOBBY);
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_lootWindowDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
                if (!gameObject.activeInHierarchy)
                    return;

                // availability may have changed (owned weapons, daily claim)
                RefreshItems();
                _uiManager.OpenWindow<LootReceivedWindow>()?.Setup(result.ReceivedLoot);
            }
            catch (OperationCanceledException)
            {
                // component destroyed mid-await, expected
            }
            finally
            {
                _isOpeningChest = false;
            }
        }

        private void CloseItemInfo()
        {
            _uiManager?.GetWindow<ItemInfoWindow>()?.Close();
            _uiManager?.GetWindow<LootReceivedWindow>()?.Close();
        }

        private void ReleaseItems()
        {
            if (_pool == null)
                return;

            foreach (var viewer in _activeViewers)
            {
                _pool.Push(viewer);
            }
            _activeViewers.Clear();
        }
    }
}
