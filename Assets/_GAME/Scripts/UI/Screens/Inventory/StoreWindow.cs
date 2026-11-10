using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class StoreWindow : UIWindow
    {
        [SerializeField] private PlaceTarget _placeTarget;
        [SerializeField] private List<InventoryTabButton> _tabButtons;
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private StoreItemViewer _itemViewerPrefab;
        [SerializeField] private GameObject _emptyStateLabel;

        private InventoryManager _inventoryManager;
        private UIManager _uiManager;
        private LocalPool<StoreItemViewer> _pool;
        private readonly List<StoreItemViewer> _activeViewers = new();
        private ItemType _selectedType;

        public override void OnOpen()
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _uiManager ??= Core.Get<UIManager>();
            _inventoryManager.PlaceTarget = _placeTarget;
            _pool ??= new LocalPool<StoreItemViewer>(_itemViewerPrefab);

            foreach (var tab in _tabButtons)
            {
                tab.Setup(SelectTab);
            }

            SelectTab(_tabButtons.Count > 0 ? _tabButtons[0].ItemType : ItemType.Weapon);

            base.OnOpen();
        }

        public override void OnClose()
        {
            CloseItemInfo();
            ReleaseItems();
            _inventoryManager.PlaceTarget = PlaceTarget.None;
            base.OnClose();
        }

        private void SelectTab(ItemType type)
        {
            _selectedType = type;

            foreach (var tab in _tabButtons)
            {
                tab.SetSelected(tab.ItemType == type);
            }

            RefreshItems();
        }

        private void RefreshItems()
        {
            ReleaseItems();

            if (_inventoryManager == null)
                return;

            var items = _inventoryManager.GetAll(_selectedType)
                .OrderForStore(item => item, _inventoryManager)
                .ToList();

            foreach (var item in items)
            {
                var viewer = _pool.Pop();
                viewer.Setup(item, () => OnItemClicked(item, (RectTransform)viewer.transform));
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
            var request = new ItemInfoRequest(itemInfo)
            {
                Anchor = anchor,
                StatsMode = EquipmentStatsMode.Current,
                Price = itemInfo.Recipe?.ItemCosts,
            };
            request.AddUseButtons(_inventoryManager, OnItemUsed);

            _uiManager.OpenWindow<ItemInfoWindow>().Setup(request);
        }

        private void OnItemUsed(ItemUseResult result)
        {
            if (!result.Success)
                Debug.LogError($"[StoreWindow] Failed to use item {result.Item.DisplayName}");
            else
                RefreshItems(); // owned marks and sorting (bought equipment goes down)

            //Feel reaction
            if (!TryShowReceivedLoot(result))
                OnItemClicked(result.Item);
        }

        private bool TryShowReceivedLoot(ItemUseResult result)
        {
            if (!result.Success || result.ReceivedLoot.Count == 0)
                return false;

            _uiManager.GetWindow<ItemInfoWindow>()?.Close();
            _uiManager.OpenWindow<LootReceivedWindow>().Setup(result.ReceivedLoot);
            return true;
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
