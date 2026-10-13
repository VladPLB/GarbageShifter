using System.Collections.Generic;

namespace _GAME.Scripts.Inventory
{
    public class ItemUseResult
    {
        public ItemInfo Item { get; }
        public ItemUsePipelineSO Action { get; }
        public bool Success { get; set; }
        public List<ItemAmount> ReceivedLoot { get; } = new();

        public ItemUseResult(ItemInfo item, ItemUsePipelineSO action)
        {
            Item = item;
            Action = action;
        }
    }
}
