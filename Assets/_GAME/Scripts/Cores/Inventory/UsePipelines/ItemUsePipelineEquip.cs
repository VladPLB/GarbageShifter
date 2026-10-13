using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/Equip", fileName = "ItemUsePipelineEquip_")]
    public class ItemUsePipelineEquip : ItemUsePipelineSO
    {
        [SerializeField] private ItemUseButtonView _equippedButtonView = new("Equipped");

        public override ItemUseButtonView GetButtonView(ItemInfo info)
        {
            return Core.Get<InventoryManager>().CanEquip(info) ? _equippedButtonView : base.GetButtonView(info);
        }

        public override bool CanExecute(ItemInfo info)
        {
            var inventoryManager = Core.Get<InventoryManager>();
            return inventoryManager.GetCount(info) > 0 && !inventoryManager.CanEquip(info);
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            return Core.Get<InventoryManager>().TryEquip(info);
        }
    }
}
