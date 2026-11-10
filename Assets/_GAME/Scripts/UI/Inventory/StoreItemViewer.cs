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
        [SerializeField] protected GameObject _priceMark;
        [Tooltip("Badge on already bought equipment")]
        [SerializeField] protected GameObject _ownedMark;
        
        public override void Setup(ItemAmount itemAmount, Action clickHandler)
        {
            base.Setup(itemAmount, clickHandler);
            bool isPriceMarkEnable = true;
            if(_priceText != null)
            {
                var recipe = itemAmount.Item.Recipe;
                if(recipe.ItemCosts.Count == 1 && recipe.ItemCosts[0].Item.Type == ItemType.Currency)
                {
                    _priceText.text = $"{recipe.ItemCosts[0].Item.DisplayName}x{recipe.ItemCosts[0].Amount}";
                    isPriceMarkEnable = false;
                }
            }
            _priceText.gameObject.SetActive(!isPriceMarkEnable);
            _priceMark.SetActive(isPriceMarkEnable);

            if (_ownedMark != null)
            {
                var item = itemAmount.Item;
                _ownedMark.SetActive(item is UpgradableItemInfo && Core.Get<InventoryManager>().GetCount(item) > 0);
            }
        }

        // Price that is not a recipe cost (e.g. localized real money price)
        public void SetPriceText(string price)
        {
            if (_priceText == null || string.IsNullOrEmpty(price))
                return;

            _priceText.text = price;
            _priceText.gameObject.SetActive(true);
            _priceMark.SetActive(false);
        }
    }
}