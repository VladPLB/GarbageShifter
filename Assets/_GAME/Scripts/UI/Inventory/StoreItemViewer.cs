using System;
using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Inventory
{
    public class StoreItemViewer: InventoryItemViewer
    {
        [SerializeField] protected TextMeshProUGUI _priceText;
        
        public override void Setup(ItemAmount itemAmount, Action clickHandler)
        {
            base.Setup(itemAmount, clickHandler);
            
            var recipe = itemAmount.Item.Recipe;
            _priceText.text = "Price:";
            foreach (var p in recipe.ItemCosts)
            {
                _priceText.text += $"\n{p.Item.DisplayName}x{p.Amount}";
            }
        }
    }
}