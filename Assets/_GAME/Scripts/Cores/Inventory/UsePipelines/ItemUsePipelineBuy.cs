using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/Buy", fileName = "ItemUsePipelineBuy_")]
    public class ItemUsePipelineBuy : ItemUsePipelineSO
    {
        [SerializeField] private ItemUseButtonView _ownedButtonView = new("Owned");

        public override ItemUseButtonView GetButtonView(ItemInfo info)
        {
            return IsOwnedEquipment(info) ? _ownedButtonView : base.GetButtonView(info);
        }

        public override bool CanExecute(ItemInfo info)
        {
            return !IsOwnedEquipment(info) && Core.Get<InventoryManager>().CanCraftByRecipe(info);
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            if (!CanExecute(info))
                return false;

            var inventoryManager = Core.Get<InventoryManager>();
            if (!inventoryManager.TryCraftByRecipe(info, out var loot))
                return false;

            result.ReceivedLoot.AddRange(loot);
            var price = info.Recipe.ItemCosts.Count > 0 ? info.Recipe.ItemCosts[0] : null;
            AnalyticsEvent.Push("shop_purchase",
                ("item_id", info.Id),
                ("item_type", info.Type.ToString()),
                ("rank", info.Rank.ToString()),
                ("place", inventoryManager.PlaceTarget.ToString()),
                ("currency", price?.Item != null ? price.Item.Id : null),
                ("price", price?.Amount ?? 0));
            return true;
        }

        // Equipment is unique: an already owned item can't be bought again
        private static bool IsOwnedEquipment(ItemInfo info)
        {
            return info is UpgradableItemInfo && Core.Get<InventoryManager>().GetCount(info) > 0;
        }
    }
}
