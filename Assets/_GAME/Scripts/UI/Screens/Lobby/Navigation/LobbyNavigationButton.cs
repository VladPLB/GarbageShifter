using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Lobby.Navigation
{
    public enum LobbyWindowType
    {
        None,
        Inventory,
        Shop,
        Store,
        Upgrade,
    }

    // Moves the lobby camera to a place and opens the window of this button there
    public class LobbyNavigationButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private LobbyPlaceType _place = LobbyPlaceType.Bar_Barmen;
        [SerializeField] private LobbyWindowType _window = LobbyWindowType.None;

        [Header("Selection")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _deselectedIconColor = Color.gray;
        [SerializeField] private Color _deselectedLabelColor = Color.gray;

        private static readonly List<LobbyNavigationButton> _active = new();

        public LobbyPlaceType Place => _place;
        public LobbyWindowType Window => _window;
        public RectTransform Rect => _button.transform as RectTransform;

        // used by the tutorial to point at the button of a place
        public static bool TryGet(LobbyPlaceType place, out LobbyNavigationButton button)
        {
            button = _active.Find(b => b.Place == place);
            return button != null;
        }

        private void OnEnable()
        {
            _active.Add(this);
        }

        private void OnDisable()
        {
            _active.Remove(this);
        }

        public void Setup(Action<LobbyNavigationButton> onClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke(this));
        }

        public void SetSelected(bool isSelected)
        {
            if (_icon != null)
                _icon.color = isSelected ? _selectedColor : _deselectedIconColor;
            if (_label != null)
                _label.color = isSelected ? _selectedColor : _deselectedLabelColor;
        }
    }
}
