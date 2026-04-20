using System;
using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Inventory
{
    public class InventoryItemViewer: MonoBehaviour, IPoolableItem
    {
        [SerializeField] protected Image _iconImage;
        [SerializeField] protected List<Color> _rangeColors;
        [SerializeField] protected TextMeshProUGUI _nameLabel;
        [SerializeField] protected TextMeshProUGUI _countLabel;
        [SerializeField] protected Button _button;
        [SerializeField] protected Image _backgroundImage;
        
        protected ItemAmount _itemAmount;
        
        public void Setup(ItemAmount itemAmount, Action clickHandler)
        {
            _itemAmount = itemAmount;
            var info = _itemAmount.Item;
            var count = _itemAmount.Amount;
            _iconImage.sprite = info.Icon;
            _backgroundImage.color = _rangeColors[(int)info.Rank];
            _nameLabel.text = info.DisplayName;
            _countLabel.text = $"x{count}";
            _button.SetListener(clickHandler);
        }
    }
}