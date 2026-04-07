using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Show/ByPlaceSO", fileName = "ItemConditionShowByPlace_")]
    public class ItemConditionShowByPlaceSO: ItemConditionShowSO
    {
        [SerializeField]
        private List<PlaceTarget> _targets = new();

        public override bool CanShow()
        {
            var inventoryManager = Core.Get<InventoryManager>();
            return _targets.Contains(inventoryManager.PlaceTarget);
        }
    }
}