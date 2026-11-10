using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class LootReceivedWindow : UIWindow
    {
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private InventoryItemViewer _itemViewerPrefab;
        [SerializeField] private GameObject _emptyStateLabel;
        [SerializeField] private Button _closeButton;

        private UIManager _uiManager;
        private LocalPool<InventoryItemViewer> _pool;
        private readonly List<InventoryItemViewer> _activeViewers = new();

        public override void OnOpen()
        {
            _uiManager ??= Core.Get<UIManager>();
            _pool ??= new LocalPool<InventoryItemViewer>(_itemViewerPrefab);

            if (_closeButton != null)
                _closeButton.SetListener(Close);

            base.OnOpen();
        }

        public override void OnClose()
        {
            CloseItemInfo();
            ReleaseItems();
            base.OnClose();
        }

        public void Setup(List<ItemAmount> loot)
        {
            ReleaseItems();

            var items = (loot ?? new List<ItemAmount>())
                .Where(a => a.Item != null && a.Amount > 0)
                .GroupBy(a => a.Item)
                .Select(g => new ItemAmount { Item = g.Key, Amount = g.Sum(a => a.Amount) })
                .ToList();

            foreach (var itemAmount in items)
            {
                var viewer = _pool.Pop();
                viewer.Setup(itemAmount, () => OnItemClicked(itemAmount, (RectTransform)viewer.transform));
                viewer.transform.SetParent(_itemsHolder, false);
                viewer.transform.localScale = Vector3.one;
                viewer.gameObject.SetActive(true);
                _activeViewers.Add(viewer);
            }

            if (_emptyStateLabel != null)
                _emptyStateLabel.SetActive(items.Count == 0);
        }

        private void OnItemClicked(ItemAmount itemAmount, RectTransform anchor = null)
        {
            var request = new ItemInfoRequest(itemAmount.Item)
            {
                Anchor = anchor,
                Amount = itemAmount.Amount,
                ShowAmount = true,
                StatsMode = EquipmentStatsMode.Current,
            };

            _uiManager.OpenWindow<ItemInfoWindow>().Setup(request);
        }

        private void CloseItemInfo()
        {
            _uiManager?.GetWindow<ItemInfoWindow>()?.Close();
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
