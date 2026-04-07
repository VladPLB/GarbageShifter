using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Use/ByPlaceSO", fileName = "ItemConditionUseByPlace_")]
    public class ItemConditionUseByPlaceSO: ItemConditionUseSO
    {
        [SerializeField]
        private List<PlaceTarget> _targets = new();

        public override bool CanUse()
        {
            var inventoryManager = Core.Get<InventoryManager>();
            return _targets.Contains(inventoryManager.PlaceTarget);
        }
    }
}