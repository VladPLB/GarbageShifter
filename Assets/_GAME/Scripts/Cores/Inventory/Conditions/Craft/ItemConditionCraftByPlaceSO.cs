using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Craft/ByPlaceSO", fileName = "ItemConditionCraftByPlace_")]
    public class ItemConditionCraftByPlaceSO: ItemConditionCraftSO
    {
        [SerializeField]
        private List<PlaceTarget> _targets = new();

        public override bool CanCraft()
        {
            var inventoryManager = Core.Get<InventoryManager>();
            return _targets.Contains(inventoryManager.PlaceTarget);
        }
    }
}