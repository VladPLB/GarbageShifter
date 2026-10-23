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
        [SerializeField] protected GameObject _upgradeMark;
        [SerializeField] protected GameObject _maxLevelMark;
        public void SetupUpgradeInfo(UpgradeResult result)
        {
            _upgradeMark.SetActive(result == UpgradeResult.Available);
            _maxLevelMark.SetActive(result == UpgradeResult.MaxLevel);
            if(_button!=null)
            {   
                _button.interactable = result != UpgradeResult.NotItem && result != UpgradeResult.NoUpgradable;
            }
        }
    }
}