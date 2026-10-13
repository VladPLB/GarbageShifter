using System.Collections.Generic;
using _GAME.Scripts.UI.Screens.Inventory.InfoSections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public abstract class ItemInfoWindowBase : UIWindow
    {
        [Header("Item")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private List<Color> _rangeColors;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _countLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private Button _closeButton;

        [Header("Sections")]
        [SerializeField] private List<ItemInfoSection> _sections = new();

        protected ItemInfoRequest Request { get; private set; }

        public override void OnOpen()
        {
            if (_closeButton != null)
                _closeButton.SetListener(Close);

            base.OnOpen();
        }

        public override void OnClose()
        {
            Request = null;
            base.OnClose();
        }

        public virtual void Setup(ItemInfoRequest request)
        {
            Request = request;
            var item = request.Item;

            _iconImage.sprite = item.Icon;
            _backgroundImage.color = _rangeColors[(int)item.Rank];
            _nameLabel.text = item.DisplayName;
            _descriptionLabel.text = item.Description;

            if (_countLabel != null)
            {
                _countLabel.gameObject.SetActive(request.ShowAmount);
                _countLabel.text = $"x{request.Amount}";
            }

            foreach (var section in _sections)
            {
                section.Setup(request);
            }
        }
    }
}
