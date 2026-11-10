using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Inventory.UsePipelines;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace _GAME.Scripts.Purchasing
{
    // In-app purchases (Google Play via Unity IAP). Products are collected from offer items with ItemUsePipelineIapPurchase
    public class PurchaseManager : MonoBehaviour, IRuntimeSetup, IDetailedStoreListener
    {
        private InventoryManager _inventoryManager;
        private IStoreController _storeController;
        private readonly Dictionary<string, (ItemInfo Item, ItemUsePipelineIapPurchase Pipeline)> _products = new();

        public bool IsInitialized => _storeController != null;

        private void Awake()
        {
            Core.Registry(this, typeof(InventoryManager));
        }

        public void RuntimeSetup()
        {
            _inventoryManager = Core.Get<InventoryManager>();
            CollectProducts();
            InitializeAsync();
        }

        private void CollectProducts()
        {
            _products.Clear();
            foreach (var item in _inventoryManager.GetAll(ItemType.Offer, true))
            {
                foreach (var pipeline in item.UsePipelines.OfType<ItemUsePipelineIapPurchase>())
                {
                    if (!string.IsNullOrEmpty(pipeline.ProductId))
                        _products[pipeline.ProductId] = (item, pipeline);
                }
            }
        }

        private async void InitializeAsync()
        {
            if (_products.Count == 0)
                return;

            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PurchaseManager] Unity Services init failed: {e.Message}");
            }

            if (this == null)
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var product in _products)
            {
                builder.AddProduct(product.Key, product.Value.Pipeline.IsConsumable ? ProductType.Consumable : ProductType.NonConsumable);
            }
            UnityPurchasing.Initialize(this, builder);
        }

        public string GetLocalizedPrice(string productId)
        {
            var product = _storeController?.products.WithID(productId);
            return product != null && product.availableToPurchase ? product.metadata.localizedPriceString : null;
        }

        // Price in the store currency (for comparing offers); null if the store is not ready
        public decimal? GetPrice(string productId)
        {
            var product = _storeController?.products.WithID(productId);
            return product != null && product.availableToPurchase ? product.metadata.localizedPrice : null;
        }

        public bool Purchase(string productId)
        {
            var product = _storeController?.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[PurchaseManager] Product '{productId}' is not available");
                return false;
            }

            _storeController.InitiatePurchase(product);
            return true;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, null);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[PurchaseManager] Initialization failed: {error} {message}");
        }

        // Also called on start for purchases that were paid but not granted yet (crash, app killed)
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var productId = purchaseEvent.purchasedProduct.definition.id;
            if (!_products.TryGetValue(productId, out var product))
            {
                Debug.LogError($"[PurchaseManager] Unknown product '{productId}'");
                return PurchaseProcessingResult.Complete;
            }

            var received = new List<ItemAmount>();
            foreach (var reward in product.Pipeline.Rewards)
            {
                // non consumables are re-delivered by the store (restore); don't grant twice
                if (!product.Pipeline.IsConsumable && _inventoryManager.GetCount(reward.Item) > 0)
                    continue;

                _inventoryManager.Add(reward.Item, reward.Amount, received);
            }

            if (received.Count > 0)
            {
                var metadata = purchaseEvent.purchasedProduct.metadata;
                AnalyticsEvent.Push("iap_purchase",
                    ("product_id", productId),
                    ("item_id", product.Item.Id),
                    ("price", (double)metadata.localizedPrice),
                    ("currency", metadata.isoCurrencyCode));
            }
            EventBus.Push(new PurchaseCompletedEvent(product.Item, received), EventBus.EventRegion.GLOBAL);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailed(product, failureReason.ToString());
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            OnPurchaseFailed(product, $"{failureDescription.reason} {failureDescription.message}");
        }

        private void OnPurchaseFailed(Product product, string reason)
        {
            var productId = product?.definition.id;
            Debug.LogWarning($"[PurchaseManager] Purchase of '{productId}' failed: {reason}");
            _products.TryGetValue(productId ?? string.Empty, out var offer);
            AnalyticsEvent.Push("iap_failed", ("product_id", productId), ("reason", reason));
            EventBus.Push(new PurchaseFailedEvent(offer.Item, reason), EventBus.EventRegion.GLOBAL);
        }

        private void OnDestroy()
        {
            Core.Unregistry(this);
        }
    }
}
