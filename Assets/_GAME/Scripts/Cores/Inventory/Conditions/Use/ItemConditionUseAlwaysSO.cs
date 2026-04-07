using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Use/AlwaysSO", fileName = "ItemConditionUseAlways")]
    public class ItemConditionUseAlwaysSO: ItemConditionUseSO
    {
        public override bool CanUse() => true;
    }
}