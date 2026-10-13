using System;
using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.UI.Inventory
{
    public class LootPreviewItemViewer : InventoryItemViewer
    {
        public void Setup(ItemInfo itemInfo, int minAmount, int maxAmount, Action clickHandler)
        {
            Setup(itemInfo, clickHandler);
            _countLabel.text = minAmount == maxAmount ? $"x{minAmount}" : $"x{minAmount}-{maxAmount}";
        }
    }
}
