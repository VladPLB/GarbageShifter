using System;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Inventory
{
    public class ItemInfoButton : MonoBehaviour, IPoolableItem
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _label;

        private Color _defaultBackgroundColor;
        private Color _defaultTextColor;
        private bool _defaultsCached;

        public void Setup(ItemUseButtonView view, bool interactable, Action clickHandler)
        {
            CacheDefaultColors();

            _label.text = view.Label;
            _label.color = view.OverrideColors ? view.TextColor : _defaultTextColor;
            if (_backgroundImage != null)
                _backgroundImage.color = view.OverrideColors ? view.BackgroundColor : _defaultBackgroundColor;

            _button.interactable = interactable;
            _button.SetListener(clickHandler);
        }

        private void CacheDefaultColors()
        {
            if (_defaultsCached)
                return;

            if (_backgroundImage != null)
                _defaultBackgroundColor = _backgroundImage.color;
            _defaultTextColor = _label.color;
            _defaultsCached = true;
        }
    }
}
