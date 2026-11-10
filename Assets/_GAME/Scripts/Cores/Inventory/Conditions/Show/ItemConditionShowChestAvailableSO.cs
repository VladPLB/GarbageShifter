using System.Collections.Generic;
using _GAME.Scripts.Inventory.Chests;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    // Hides a chest in the shop when its pool can't give the full amount of unique rewards
    [CreateAssetMenu(menuName = "Inventory/Condition/Show/ChestAvailableSO", fileName = "ItemConditionShowChestAvailable_")]
    public class ItemConditionShowChestAvailableSO : ItemConditionShowSO
    {
        [SerializeField] private ChestRewardPoolSO _pool;
        [SerializeField, Min(1)] private int _rewardsCount = 1;
        [SerializeField] private List<PlaceTarget> _checkedPlaces = new() { PlaceTarget.Shop, PlaceTarget.Store };

        public override bool CanShow()
        {
            var inventoryManager = Core.Get<InventoryManager>();
            if (!_checkedPlaces.Contains(inventoryManager.PlaceTarget))
                return true;

            return _pool != null && _pool.GetAvailableCount(inventoryManager) >= _rewardsCount;
        }
    }
}
