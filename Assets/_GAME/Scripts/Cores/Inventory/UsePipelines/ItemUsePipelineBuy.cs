using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/Buy", fileName = "ItemUsePipelineBuy_")]
    public class ItemUsePipelineBuy : ItemUsePipelineSO
    {
        public override bool CanExecute(ItemInfo info)
        {
            return Core.Get<InventoryManager>().CanCraftByRecipe(info);
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            if (!Core.Get<InventoryManager>().TryCraftByRecipe(info, out var loot))
                return false;

            result.ReceivedLoot.AddRange(loot);
            return true;
        }
    }
}
