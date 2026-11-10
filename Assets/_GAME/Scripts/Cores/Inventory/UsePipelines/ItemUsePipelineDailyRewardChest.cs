using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    // Free chest once per local calendar day (resets at midnight, not 24h after the last claim)
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/DailyRewardChest", fileName = "ItemUsePipelineDailyRewardChest_")]
    public class ItemUsePipelineDailyRewardChest : ItemUsePipelineOpenRewardChest
    {
        [SerializeField] private ItemUseButtonView _claimedButtonView = new("Tomorrow");

        public override ItemUseButtonView GetButtonView(ItemInfo info)
        {
            return Core.Get<InventoryManager>().IsDailyClaimAvailable(info.Id) ? base.GetButtonView(info) : _claimedButtonView;
        }

        protected override string Source => "daily";

        protected override bool CanPay(ItemInfo info, InventoryManager inventory) => inventory.IsDailyClaimAvailable(info.Id);

        protected override bool Pay(ItemInfo info, InventoryManager inventory)
        {
            inventory.MarkDailyClaimed(info.Id);
            return true;
        }
    }
}
