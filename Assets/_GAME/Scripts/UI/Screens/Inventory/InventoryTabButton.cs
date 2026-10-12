using System;
using _GAME.Scripts.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class InventoryTabButton : MonoBehaviour
    {
        [SerializeField] private ItemType _itemType;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _icon;

        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _deselectedColor = Color.gray;

        public ItemType ItemType => _itemType;

        public void Setup(Action<ItemType> onClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(_itemType));
        }

        public void SetSelected(bool selected)
        {
            var color = selected ? _selectedColor : _deselectedColor;
            if (_label != null) _label.color = color;
            if (_icon != null) _icon.color = color;
        }
    }
}
