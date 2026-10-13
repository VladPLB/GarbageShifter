using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class InventoryWindow : UIWindow
    {
        [SerializeField] private List<InventoryTabButton> _tabButtons;
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private InventoryItemViewer _itemViewerPrefab;
        [SerializeField] private GameObject _emptyStateLabel;

        private InventoryManager _inventoryManager;
        private DataBase _dataBase;
        private UIManager _uiManager;
        private LocalPool<InventoryItemViewer> _pool;
        private readonly List<InventoryItemViewer> _activeViewers = new();
        private ItemType _selectedType;

        public override void OnOpen()
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _dataBase ??= Core.Get<DataBase>();
            _uiManager ??= Core.Get<UIManager>();
            _inventoryManager.PlaceTarget = PlaceTarget.Inventory;
            _pool ??= new LocalPool<InventoryItemViewer>(_itemViewerPrefab);

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

            var items = _inventoryManager.GetMyInventory(_selectedType)
                .OrderBy(kv => kv.Key.DisplayName)
                .ToList();

            foreach (var kv in items)
            {
                var item = kv.Key;
                var itemAmount = new ItemAmount { Item = item, Amount = kv.Value };
                
                
                var viewer = _pool.Pop();
                viewer.Setup(itemAmount, () => OnItemClicked(itemAmount));
                if (item is UpgradableItemInfo)
                {
                    var level = _inventoryManager.GetLevel(item);
                    var data = _dataBase.Weapons.GetData(item.Id);
                    data.Setup(null, level);
                    viewer.SetupEquipmentInfo(data, _inventoryManager.CanEquip(item));
                }
                viewer.transform.SetParent(_itemsHolder, false);
                viewer.transform.localScale = Vector3.one;
                viewer.gameObject.SetActive(true);
                _activeViewers.Add(viewer);
            }

            if (_emptyStateLabel != null)
                _emptyStateLabel.SetActive(items.Count == 0);
        }

        private void OnItemClicked(ItemAmount itemAmount)
        {
            var item = itemAmount.Item;
            var request = new ItemInfoRequest(item)
            {
                Amount = itemAmount.Amount,
                ShowAmount = item is not UpgradableItemInfo,
                StatsMode = EquipmentStatsMode.Current,
            };

            request.AddUseButtons(_inventoryManager, OnItemUsed);

            _uiManager.OpenWindow<ItemInfoWindow>().Setup(request);
        }

        private void OnItemUsed(ItemUseResult result)
        {
            RefreshItems();

            if (TryShowReceivedLoot(result))
                return;

            var count = _inventoryManager.GetCount(result.Item);
            if (count > 0)
                OnItemClicked(new ItemAmount { Item = result.Item, Amount = count });
            else
                _uiManager.GetWindow<ItemInfoWindow>()?.Close();
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
