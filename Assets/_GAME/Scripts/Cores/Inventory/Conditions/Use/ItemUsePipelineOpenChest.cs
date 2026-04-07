using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Save;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/UsePipelines/OpenChest", fileName = "ItemUsePipelineOpenChest_")]
    public class ItemUsePipelineOpenChest: ItemUsePipelineSO
    {
        [SerializeField]
        private int _spendAmount = 1;
        [SerializeField]
        private LootTable _lootTable;
        
        public override bool Use(ItemInfo info)
        {
            var inventoryManager = Core.Get<InventoryManager>();
            if (inventoryManager.GetCount(info) >= _spendAmount)
            {
                inventoryManager.Remove(info, _spendAmount);
                var loot = inventoryManager.UnpackLootTable(_lootTable);
                if (loot != null && loot.Count > 0)
                {
                    inventoryManager.Add(loot);
                }
            }

            return false;
        }
    }
}