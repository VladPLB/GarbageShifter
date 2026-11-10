using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    // Store chest: pays the recipe price and opens immediately (the chest never goes to the inventory)
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/BuyRewardChest", fileName = "ItemUsePipelineBuyRewardChest_")]
    public class ItemUsePipelineBuyRewardChest : ItemUsePipelineOpenRewardChest
    {
        protected override string Source => "buy";

        protected override bool CanPay(ItemInfo info, InventoryManager inventory) => inventory.CanCraftByRecipe(info);
        protected override bool Pay(ItemInfo info, InventoryManager inventory) => inventory.ConsumeRecipeIngredients(info);
    }
}
