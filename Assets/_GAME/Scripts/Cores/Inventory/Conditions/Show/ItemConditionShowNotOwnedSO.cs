using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    // Shown only while the player doesn't own the item (e.g. NoAds offer after the purchase)
    [CreateAssetMenu(menuName = "Inventory/Condition/Show/NotOwnedSO", fileName = "ItemConditionShowNotOwned_")]
    public class ItemConditionShowNotOwnedSO : ItemConditionShowSO
    {
        [SerializeField] private ItemInfo _item;

        public override bool CanShow()
        {
            return _item == null || Core.Get<InventoryManager>().GetCount(_item) <= 0;
        }
    }
}
