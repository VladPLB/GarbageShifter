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
    public class InventoryItemViewer: MonoBehaviour, IPoolableItem
    {
        [SerializeField] protected Image _iconImage;
        [SerializeField] protected List<Color> _rangeColors;
        [SerializeField] protected TextMeshProUGUI _nameLabel;
        [SerializeField] protected TextMeshProUGUI _countLabel;
        [SerializeField] protected Button _button;
        [SerializeField] protected Image _backgroundImage;
        [SerializeField] protected GameObject _equipedMark;
        
        protected ItemAmount _itemAmount;
        
        public virtual void Setup(ItemAmount itemAmount, Action clickHandler)
        {
            _itemAmount = itemAmount;
            var info = _itemAmount.Item;
            var count = _itemAmount.Amount;
            _iconImage.sprite = info.Icon;
            _backgroundImage.color = _rangeColors[(int)info.Rank];
            
            if(_nameLabel!=null)
            {
                _nameLabel.text = info.DisplayName;
            }
            if(_countLabel != null)
            {
                _countLabel.text =count>1? $"x{count}": String.Empty;
            }
            if(_button!=null)
            {
                _button.SetListener(clickHandler);
            }
            if(_equipedMark!=null)
            {
                _equipedMark.SetActive(false);
            }
        }
        
        public void Setup(ItemInfo itemInfo, Action clickHandler)
        {
            Setup(new ItemAmount()
            {
                Item = itemInfo,
                Amount = 1
            }, clickHandler);
        }

        public void SetInteractable(bool interactable)
        {
            if(_button!=null)
            {
                _button.interactable = interactable;
            }
        }

        public virtual void SetupEquipmentInfo(WeaponData data, bool isEquiped)
        {
            if(_equipedMark!=null)
            {
                _equipedMark.SetActive(isEquiped);
            }
            _nameLabel.text += $" {data.Level}lv.";
        }
    }
}