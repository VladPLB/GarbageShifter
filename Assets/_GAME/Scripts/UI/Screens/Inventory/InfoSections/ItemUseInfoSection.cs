using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory.InfoSections
{
    public abstract class ItemUseInfoSection<T> : ItemInfoSection where T : ItemUseInfo
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private List<ItemType> _itemTypes = new() { ItemType.Consumable };

        private InventoryManager _inventoryManager;

        public sealed override void Setup(ItemInfoRequest request)
        {
            _inventoryManager ??= Core.Get<InventoryManager>();

            var infos = IsSupported(request.Item)
                ? _inventoryManager.GetUseInfos<T>(request.Item)
                : new List<T>();

            var root = _root != null ? _root : gameObject;
            root.SetActive(infos.Count > 0);

            if (infos.Count > 0)
                Show(request, infos);
        }

        protected abstract void Show(ItemInfoRequest request, List<T> infos);

        private bool IsSupported(ItemInfo item)
        {
            return _itemTypes.Count == 0 || _itemTypes.Contains(item.Type);
        }
    }
}
