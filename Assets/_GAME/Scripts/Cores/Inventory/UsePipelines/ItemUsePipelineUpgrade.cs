using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/Upgrade", fileName = "ItemUsePipelineUpgrade_")]
    public class ItemUsePipelineUpgrade : ItemUsePipelineSO
    {
        [SerializeField] private ItemUseButtonView _maxLevelButtonView = new("Max Level");

        public override ItemUseButtonView GetButtonView(ItemInfo info)
        {
            Core.Get<InventoryManager>().TryCanUpgrade(info, out var result, out _);
            return result == UpgradeResult.MaxLevel ? _maxLevelButtonView : base.GetButtonView(info);
        }

        public override bool CanExecute(ItemInfo info)
        {
            return Core.Get<InventoryManager>().TryCanUpgrade(info, out _, out _);
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            return Core.Get<InventoryManager>().TryUpgrade(info, out _, out _);
        }
    }
}
