using System;
using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Inventory
{
    public class CostItemViewer : InventoryItemViewer
    {
        public void Setup(ItemInfo itemInfo, int have, int need, Color countColor, Action clickHandler)
        {
            Setup(itemInfo, clickHandler);
            _countLabel.text = $"{have}/{need}";
            _countLabel.color = countColor;
        }
    }
}
