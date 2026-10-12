using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Inventory
{
    public class UpgradeItemViewer: InventoryItemViewer
    {
        [SerializeField] protected TextMeshProUGUI _priceText;
        [SerializeField] protected TextMeshProUGUI _upgradeText;
        
        public override void Setup(ItemAmount itemAmount, Action clickHandler)
        {
            base.Setup(itemAmount, clickHandler);
        }

        public void SetupInfo(WeaponData weaponData)
        {
            _upgradeText.text = weaponData.GetStatsLabelForUpgrade();
            if(weaponData.Level < weaponData.MaxLevel)
            {
                var price = ((UpgradableItemInfo)_itemAmount.Item).GetPriceByLevel(weaponData.Level);
                _priceText.text = "Price:";
                foreach (var p in price)
                {
                    _priceText.text += $"\n{p.Item.DisplayName}x{p.Amount} ";
                }
            }
            else
            {
                _button.onClick.RemoveAllListeners();
                _priceText.text = "Max Level!";
            }
        }
    }
}