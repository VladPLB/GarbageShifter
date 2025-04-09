using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class BottomPanelRoomButton : MonoBehaviour, IPlaceButton
    {
        [SerializeField] protected Button _button;
        [SerializeField] protected List<LobbyCameraType> _types;
        [SerializeField] protected Image _icon;
        [SerializeField] protected TextMeshProUGUI _label;
        
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _deselectedIconColor;
        [SerializeField] private Color _deselectedLabelColor;
        
        public List<LobbyCameraType> Types => _types;

        public void Setup(Action<List<LobbyCameraType>> onClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(()=>onClick?.Invoke(Types));
        }

        public void Select(LobbyCameraType type)
        {
            bool selected = _types.Contains(type);
            if (_icon != null)
            {
                _icon.color = selected ? _selectedColor : _deselectedIconColor;
            }
            if (_label != null)
            {
                _label.color = selected ? _selectedColor : _deselectedLabelColor;
            }
        }
    }
}