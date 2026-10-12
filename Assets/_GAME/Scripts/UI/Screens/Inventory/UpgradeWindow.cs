using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class UpgradeWindow : UIWindow
    {
        [SerializeField] private PlaceTarget _placeTarget;
        [SerializeField] private List<InventoryTabButton> _tabButtons;
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private UpgradeItemViewer _itemViewerPrefab;
        [SerializeField] private GameObject _emptyStateLabel;

        private InventoryManager _inventoryManager;
        private DataBase _dataBase;
        private UIManager _uiManager;
        private LocalPool<UpgradeItemViewer> _pool;
        private readonly List<UpgradeItemViewer> _activeViewers = new();
        private ItemType _selectedType;

        public override void OnOpen()
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _dataBase ??= Core.Get<DataBase>();
            _uiManager ??= Core.Get<UIManager>();

            _inventoryManager.PlaceTarget = _placeTarget;
            _pool ??= new LocalPool<UpgradeItemViewer>(_itemViewerPrefab);

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

            var items = _inventoryManager.GetMyInventory(_selectedType).ToList();
            
            foreach (var pair in items)
            {
                var viewer = _pool.Pop();
                var item = pair.Key;
                int level = _inventoryManager.GetLevel(item);
                var data = _dataBase.Weapons.GetData(item.Id);
                data.Setup(null, level);
                viewer.Setup(item, () => OnItemClicked(item));
                viewer.SetupInfo(data);
                viewer.transform.SetParent(_itemsHolder, false);
                viewer.transform.localScale = Vector3.one;
                viewer.gameObject.SetActive(true);
                _activeViewers.Add(viewer);
            }

            if (_emptyStateLabel != null)
                _emptyStateLabel.SetActive(items.Count == 0);
        }

        private void OnItemClicked(ItemInfo itemInfo)
        {
            var request = new EquipmentInfoRequest(itemInfo)
            {
                StatsMode = EquipmentStatsMode.UpgradePreview,
            };

            var canUpgrade = _inventoryManager.TryCanUpgrade(itemInfo, out var result, out _);
            if (result == UpgradeResult.MaxLevel)
            {
                request.PriceOverrideText = "Max Level!";
            }
            else if (itemInfo is UpgradableItemInfo upgradable)
            {
                request.Price = upgradable.GetPriceByLevel(_inventoryManager.GetLevel(itemInfo));
                request.AddButton("Upgrade", () => OnUpgradeClicked(itemInfo), canUpgrade);
            }

            _uiManager.OpenWindow<EquipmentInfoWindow>().Setup(request);
        }

        private void OnUpgradeClicked(ItemInfo itemInfo)
        {
            if (_inventoryManager.TryUpgrade(itemInfo, out var results, out var missingIngredients))
            {
                RefreshItems();
                OnItemClicked(itemInfo);
                //Feel reaction
            }
            else
            {
                Debug.LogError($"[UpgradeWindow] Failed to upgrade {itemInfo.DisplayName} : {results}");
            }
        }

        private void CloseItemInfo()
        {
            _uiManager?.GetWindow<EquipmentInfoWindow>()?.Close();
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
