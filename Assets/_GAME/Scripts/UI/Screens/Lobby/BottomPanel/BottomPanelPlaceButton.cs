using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class BottomPanelPlaceButton : MonoBehaviour, IPlaceButton
    {
        [SerializeField] protected Button _button;
        [SerializeField] protected TextMeshProUGUI _label;
        
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _deselectedColor;
        
        protected LobbyCameraType _type;

        public void Setup(LobbyCameraType type, Action<LobbyCameraType> onClick)
        {
            _type = type;
            var text = type switch
            {
                LobbyCameraType.Bar_Barmen => "Barmen",
                LobbyCameraType.Bar_BlackMarket => "Black Market",
                LobbyCameraType.Angar_Squad => "Squad",
                LobbyCameraType.Angar_Ship => "Ship",
                LobbyCameraType.Angar_Master => "Upgrade",
                LobbyCameraType.Lab_Researcher => "Research",
                LobbyCameraType.Lab_Medic => "Medic",
                LobbyCameraType.Shop_Market => "Shop",
                LobbyCameraType.Shop_Teleport => "Teleport",
               _ => string.Empty
            };
            _button.onClick.RemoveAllListeners();
            bool isActive = !string.IsNullOrEmpty(text);
            gameObject.SetActive(isActive);
            if (isActive)
            {
                _label.text = text;
                _button.onClick.AddListener(()=>onClick?.Invoke(_type));
            }
        }

        public void Select(LobbyCameraType type)
        {
            bool selected = type == _type;
            if (_label != null)
            {
                _label.color = selected ? _selectedColor : _deselectedColor;
            }
        }
    }
}