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
        
        protected LobbyPlaceType _type;

        public void Setup(LobbyPlaceType type, Action<LobbyPlaceType> onClick)
        {
            _type = type;
            var text = type switch
            {
                LobbyPlaceType.Bar_Barmen => "Barmen",
                LobbyPlaceType.Bar_BlackMarket => "Black Market",
                LobbyPlaceType.Angar_Squad => "Squad",
                LobbyPlaceType.Angar_Ship => "Ship",
                LobbyPlaceType.Angar_Master => "Upgrade",
                LobbyPlaceType.Lab_Researcher => "Research",
                LobbyPlaceType.Lab_Medic => "Medic",
                LobbyPlaceType.Shop_Market => "Shop",
                LobbyPlaceType.Shop_Teleport => "Teleport",
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

        public void Select(LobbyPlaceType type)
        {
            bool selected = type == _type;
            if (_label != null)
            {
                _label.color = selected ? _selectedColor : _deselectedColor;
            }
        }
    }
}