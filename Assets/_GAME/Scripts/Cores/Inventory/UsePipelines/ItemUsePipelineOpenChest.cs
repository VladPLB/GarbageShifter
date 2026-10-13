using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Inventory.UsePipelines
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/OpenChest", fileName = "ItemUsePipelineOpenChest_")]
    public class ItemUsePipelineOpenChest: ItemUsePipelineSO
    {
        [SerializeField]
        private int _spendAmount = 1;
        [SerializeField]
        private LootTable _lootTable;

        public override void CollectUseInfos(ItemInfo info, List<ItemUseInfo> infos)
        {
            base.CollectUseInfos(info, infos);
            infos.Add(new ItemUseLootInfo(_lootTable));
        }

        public override bool CanExecute(ItemInfo info)
        {
            return Core.Get<InventoryManager>().GetCount(info) >= _spendAmount;
        }

        public override bool Use(ItemInfo info, ItemUseResult result)
        {
            var inventoryManager = Core.Get<InventoryManager>();
            if (!inventoryManager.Remove(info, _spendAmount))
            {
                return false;
            }

            result.ReceivedLoot.AddRange(inventoryManager.UnpackLootTable(_lootTable));
            return true;
        }
    }
}
