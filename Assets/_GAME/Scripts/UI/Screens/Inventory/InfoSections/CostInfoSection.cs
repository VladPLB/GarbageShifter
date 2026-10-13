using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using TMPro;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory.InfoSections
{
    public class CostInfoSection : ItemInfoSection
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private CostItemViewer _itemViewerPrefab;
        [SerializeField] private Color _enoughColor = Color.green;
        [SerializeField] private Color _missingColor = Color.red;

        private InventoryManager _inventoryManager;
        private LocalPool<CostItemViewer> _pool;
        private readonly List<CostItemViewer> _activeViewers = new();

        public override void Setup(ItemInfoRequest request)
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _pool ??= new LocalPool<CostItemViewer>(_itemViewerPrefab);
            ReleaseItems();

            var hasOverride = !string.IsNullOrEmpty(request.PriceOverrideText);
            var hasPrice = request.Price != null && request.Price.Count > 0;

            var root = _root != null ? _root : gameObject;
            root.SetActive(hasOverride || hasPrice);

            if (hasOverride)
            {
                _titleLabel.text = request.PriceOverrideText;
                return;
            }

            if (!hasPrice)
                return;

            _titleLabel.text = request.PriceTitle;
            foreach (var cost in request.Price)
            {
                if (cost.Item == null)
                    continue;

                var item = cost.Item;
                var have = _inventoryManager.GetCount(item);
                var color = have >= cost.Amount ? _enoughColor : _missingColor;

                var viewer = _pool.Pop();
                viewer.Setup(item, have, cost.Amount, color, () => ItemPreviewWindow.Open(item));
                viewer.SetInteractable(request.AllowPreview);
                viewer.transform.SetParent(_itemsHolder, false);
                viewer.transform.localScale = Vector3.one;
                viewer.gameObject.SetActive(true);
                _activeViewers.Add(viewer);
            }
        }

        private void ReleaseItems()
        {
            foreach (var viewer in _activeViewers)
            {
                _pool.Push(viewer);
            }
            _activeViewers.Clear();
        }
    }
}
