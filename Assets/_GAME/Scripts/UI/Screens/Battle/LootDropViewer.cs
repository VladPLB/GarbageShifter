using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class LootDropViewer: MonoBehaviour
    {
        private const int MAX_STORAGE_CAPACITY = 8;
        
        [SerializeField] protected RectTransform _holder;
        [SerializeField] protected InventoryItemViewer _itemViewerPrefabReference;
        [SerializeField] protected ScrollRect _lootScrollRect;
        [SerializeField] protected List<RectTransform> _outItemHolders;
        [SerializeField] protected Button _addCapacityButton;
        
        LocalPool<InventoryItemViewer> _pool;
        
        private List<InventoryItemViewer> _lootItemViewers = new List<InventoryItemViewer>();
        private List<InventoryItemViewer> _outItemViewers = new List<InventoryItemViewer>();
        
        private LocalInventory _inventory = null;
        
        public void Setup(LocalInventory inventory)
        {
            _inventory = inventory;

            _pool = new LocalPool<InventoryItemViewer>(_itemViewerPrefabReference);
            _addCapacityButton.SetListener(AddCapacity);
            
            UpdateStorageHoldersVisibility();

            
            _inventory.TryToOut();
            UpdateLootAndOutItems();
        }

        private void UpdateLootAndOutItems()
        {
            ReleaseItems();
            var lootItems = _inventory.GetLootItems();
            var outItems = _inventory.GetOutItems();

            for(int i = 0; i< lootItems.Count;i++)
            {
                var item = _pool.Pop();
                var ind = i;
                item.Setup(lootItems[i], ()=> ClickLootItem(ind));
                item.transform.SetParent(_lootScrollRect.content, false);
                item.transform.localScale = Vector3.one;
                item.gameObject.SetActive(true);
                _lootItemViewers.Add(item);
            }
            
            for(int i = 0; i< outItems.Count;i++)
            {
                var item = _pool.Pop();
                var ind = i;
                item.Setup(outItems[i], ()=> ClickOutItem(ind));
                item.transform.SetParent(_outItemHolders[i], false);
                item.transform.localScale = Vector3.one;
                var rect = (item.transform as RectTransform);
                rect.anchorMin = rect.anchorMax= Vector3.one * .5f;
                rect.anchoredPosition = Vector3.zero;
                item.gameObject.SetActive(true);
                _outItemViewers.Add(item);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_holder);
        }

        private void ClickLootItem(int index)
        {
            if(_inventory.TryToOut(index))
            {
                UpdateLootAndOutItems();
            }
        }
        
        private void ClickOutItem(int index)
        {
            if(_inventory.RemoveFromOut(index))
            {
                UpdateLootAndOutItems();
            }
        }

        private void UpdateStorageHoldersVisibility()
        {
            for (int i = 0; i < _outItemHolders.Count; i++)
            {
                _outItemHolders[i].gameObject.SetActive(i<_inventory.Capacity);
            }
            _addCapacityButton.gameObject.SetActive(_inventory.Capacity < MAX_STORAGE_CAPACITY);
        }

        private void AddCapacity()
        {
            if(_inventory.Capacity<MAX_STORAGE_CAPACITY)
            {
                _inventory.AddCapacity();
                UpdateStorageHoldersVisibility();
            }
        }

        private void ReleaseItems()
        {
            foreach (var lItem in _lootItemViewers)
            {
                _pool.Push(lItem);
            }
            
            _lootItemViewers.Clear();
            
            foreach (var oItem in _outItemViewers)
            {
                _pool.Push(oItem);
            }
            
            _outItemViewers.Clear();
        }

        public void Release()
        {
            ReleaseItems();
            _addCapacityButton.onClick.RemoveAllListeners();
        }
    }
}