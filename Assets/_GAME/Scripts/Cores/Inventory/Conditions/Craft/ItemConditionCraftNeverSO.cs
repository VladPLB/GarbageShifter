using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Craft/NewerSO", fileName = "ItemConditionCraftNewer")]
    public class ItemConditionCraftNeverSO: ItemConditionCraftSO
    {
        public override bool CanCraft() => false;
    }
}