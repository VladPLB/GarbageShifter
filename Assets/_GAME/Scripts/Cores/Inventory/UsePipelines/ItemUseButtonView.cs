using System;
using UnityEngine;

namespace _GAME.Scripts.Inventory
{
    [Serializable]
    public class ItemUseButtonView
    {
        [SerializeField] private string _label = "Use";
        [SerializeField] private bool _overrideColors;
        [SerializeField] private Color _backgroundColor = Color.white;
        [SerializeField] private Color _textColor = Color.black;

        public string Label => _label;
        public bool OverrideColors => _overrideColors;
        public Color BackgroundColor => _backgroundColor;
        public Color TextColor => _textColor;

        public ItemUseButtonView()
        {
        }

        public ItemUseButtonView(string label)
        {
            _label = label;
        }
    }
}
