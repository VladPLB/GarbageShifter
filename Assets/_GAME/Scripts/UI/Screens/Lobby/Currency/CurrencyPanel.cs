using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby.Currency
{
    public class CurrencyPanel : LobbyPanel
    {
        [Header("Currencies")]
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private CurrencyViewer _viewerPrefab;
        [SerializeField] private ItemType _itemType = ItemType.Currency;
        [SerializeField] private bool _hideEmpty;

        private InventoryManager _inventoryManager;
        private LocalPool<CurrencyViewer> _pool;
        private readonly Dictionary<ItemInfo, CurrencyViewer> _viewers = new();

        public void Initialize()
        {
            _inventoryManager = Core.Get<InventoryManager>();
            _pool ??= new LocalPool<CurrencyViewer>(_viewerPrefab);

            EventBus.Subscribe<InventoryItemChangedEvent>(OnItemChanged, EventBus.EventRegion.GLOBAL);
            EventBus.Subscribe<PlaceTargetChangedEvent>(OnPlaceTargetChanged, EventBus.EventRegion.GLOBAL);

            Rebuild();

            _holderTransform.anchoredPosition = _hidePosition;
            Show().Forget();
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<InventoryItemChangedEvent>(OnItemChanged, EventBus.EventRegion.GLOBAL);
            EventBus.Unsubscribe<PlaceTargetChangedEvent>(OnPlaceTargetChanged, EventBus.EventRegion.GLOBAL);
            base.OnDestroy();
        }

        private void OnItemChanged(InventoryItemChangedEvent e)
        {
            if (e.Item == null || e.Item.Type != _itemType)
                return;

            if (_viewers.TryGetValue(e.Item, out var viewer) && (!_hideEmpty || e.CurrentAmount > 0))
                viewer.ChangeAmount(e.CurrentAmount);
            else
                Rebuild();
        }

        private void OnPlaceTargetChanged(PlaceTargetChangedEvent e)
        {
            Rebuild();
        }

        private void Rebuild()
        {
            var items = _inventoryManager.ItemDatabase.GetByType(_itemType)
                .Where(_inventoryManager.CanShow)
                .Where(item => !_hideEmpty || _inventoryManager.GetCount(item) > 0)
                .ToList();

            foreach (var item in _viewers.Keys.Except(items).ToList())
            {
                _pool.Push(_viewers[item]);
                _viewers.Remove(item);
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!_viewers.TryGetValue(item, out var viewer))
                {
                    viewer = _pool.Pop();
                    viewer.Setup(item, _inventoryManager.GetCount(item));
                    viewer.transform.SetParent(_itemsHolder, false);
                    viewer.transform.localScale = Vector3.one;
                    viewer.gameObject.SetActive(true);
                    _viewers.Add(item, viewer);
                }
                viewer.transform.SetSiblingIndex(i);
            }
        }
    }
}
