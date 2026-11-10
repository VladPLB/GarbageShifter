using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Show/NeverSO", fileName = "ItemConditionShowNever")]
    public class ItemConditionShowNeverSO: ItemConditionShowSO
    {
        public override bool CanShow() => false;
    }
}
