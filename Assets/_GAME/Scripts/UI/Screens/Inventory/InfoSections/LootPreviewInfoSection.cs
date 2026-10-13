using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory.InfoSections
{
    public class LootPreviewInfoSection : ItemUseInfoSection<ItemUseLootInfo>
    {
        [SerializeField] private RectTransform _itemsHolder;
        [SerializeField] private LootPreviewItemViewer _itemViewerPrefab;

        private LocalPool<LootPreviewItemViewer> _pool;
        private readonly List<LootPreviewItemViewer> _activeViewers = new();

        protected override void Show(ItemInfoRequest request, List<ItemUseLootInfo> infos)
        {
            _pool ??= new LocalPool<LootPreviewItemViewer>(_itemViewerPrefab);
            ReleaseItems();

            var rewards = infos
                .Where(i => i.Loot?.Entries != null)
                .SelectMany(i => i.Loot.Entries)
                .Where(e => e.Item != null)
                .GroupBy(e => e.Item)
                .Select(g => (Item: g.Key, Min: g.Min(e => e.MinAmount), Max: g.Max(e => e.MaxAmount)));

            foreach (var reward in rewards)
            {
                var item = reward.Item;
                var viewer = _pool.Pop();
                viewer.Setup(item, reward.Min, reward.Max, () => ItemPreviewWindow.Open(item));
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
