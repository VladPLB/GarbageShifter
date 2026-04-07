using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Show/AlwaysSO", fileName = "ItemConditionShowAlways")]
    public class ItemConditionShowAlwaysSO: ItemConditionShowSO
    {
        public override bool CanShow() => true;
    }
}