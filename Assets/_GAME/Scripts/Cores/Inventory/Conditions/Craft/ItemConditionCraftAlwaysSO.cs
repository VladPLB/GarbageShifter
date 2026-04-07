using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Craft/AlwaysSO", fileName = "ItemConditionCraftAlways")]
    public class ItemConditionCraftAlwaysSO: ItemConditionCraftSO
    {
        public override bool CanCraft() => true;
    }
}