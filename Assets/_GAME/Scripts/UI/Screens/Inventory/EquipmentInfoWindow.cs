using System.Collections.Generic;
using _GAME.Scripts.Battle.Weapons;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class EquipmentInfoWindow : UIWindow
    {
        [Header("Item")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private List<Color> _rangeColors;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _countLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private GameObject _equippedMark;

        [Header("Stats")]
        [SerializeField] private GameObject _statsRoot;
        [SerializeField] private TextMeshProUGUI _statsLabel;

        [Header("Price")]
        [SerializeField] private GameObject _priceRoot;
        [SerializeField] private TextMeshProUGUI _priceLabel;
        [SerializeField] private Color _missingPriceColor = Color.red;

        [Header("Buttons")]
        [SerializeField] private RectTransform _buttonsHolder;
        [SerializeField] private EquipmentInfoButton _buttonPrefab;
        [SerializeField] private Button _closeButton;

        private InventoryManager _inventoryManager;
        private DataBase _dataBase;
        private LocalPool<EquipmentInfoButton> _pool;
        private readonly List<EquipmentInfoButton> _activeButtons = new();
        private EquipmentInfoRequest _request;

        public ItemInfo Item => _request?.Item;

        public override void OnOpen()
        {
            _inventoryManager ??= Core.Get<InventoryManager>();
            _dataBase ??= Core.Get<DataBase>();
            _pool ??= new LocalPool<EquipmentInfoButton>(_buttonPrefab);

            if (_closeButton != null)
                _closeButton.SetListener(Close);

            base.OnOpen();
        }

        public override void OnClose()
        {
            ReleaseButtons();
            _request = null;
            base.OnClose();
        }

        public void Setup(EquipmentInfoRequest request)
        {
            _request = request;
            var item = request.Item;

            _iconImage.sprite = item.Icon;
            _backgroundImage.color = _rangeColors[(int)item.Rank];
            _descriptionLabel.text = item.Description;

            if (_countLabel != null)
            {
                _countLabel.gameObject.SetActive(request.ShowAmount);
                _countLabel.text = $"x{request.Amount}";
            }

            _nameLabel.text = item.DisplayName;

            if (_equippedMark != null)
                _equippedMark.SetActive(_inventoryManager.CanEquip(item));

            SetupStats(GetWeaponData(item), request.StatsMode);
            SetupPrice(request);
            SetupButtons(request);
        }

        private WeaponData GetWeaponData(ItemInfo item)
        {
            if (item is not UpgradableItemInfo)
                return null;

            var data = _dataBase.Weapons.GetData(item.Id);
            data?.Setup(null, _inventoryManager.GetLevel(item));
            return data;
        }

        private void SetupStats(WeaponData data, EquipmentStatsMode mode)
        {
            var label = data == null ? null : mode switch
            {
                EquipmentStatsMode.Current => data.GetStatsLabel(),
                EquipmentStatsMode.UpgradePreview => data.Level < data.MaxLevel
                    ? data.GetStatsLabelForUpgrade()
                    : data.GetStatsLabel(),
                _ => null,
            };

            _statsRoot.SetActive(!string.IsNullOrEmpty(label));
            _statsLabel.text = label;
        }

        private void SetupPrice(EquipmentInfoRequest request)
        {
            if (!string.IsNullOrEmpty(request.PriceOverrideText))
            {
                _priceRoot.SetActive(true);
                _priceLabel.text = request.PriceOverrideText;
                return;
            }

            if (request.Price == null || request.Price.Count == 0)
            {
                _priceRoot.SetActive(false);
                return;
            }

            _priceRoot.SetActive(true);
            var missingColor = ColorUtility.ToHtmlStringRGB(_missingPriceColor);
            var text = request.PriceTitle;
            foreach (var cost in request.Price)
            {
                var have = _inventoryManager.GetCount(cost.Item);
                var line = $"{cost.Item.DisplayName} {have}/{cost.Amount}";
                text += have < cost.Amount ? $"\n<color=#{missingColor}>{line}</color>" : $"\n{line}";
            }
            _priceLabel.text = text;
        }

        private void SetupButtons(EquipmentInfoRequest request)
        {
            ReleaseButtons();

            foreach (var option in request.Buttons)
            {
                var button = _pool.Pop();
                button.Setup(option.Label, option.Interactable, () => OnButtonClicked(option));
                button.transform.SetParent(_buttonsHolder, false);
                button.transform.localScale = Vector3.one;
                button.gameObject.SetActive(true);
                _activeButtons.Add(button);
            }
        }

        private void OnButtonClicked(EquipmentInfoButtonOption option)
        {
            if (option.CloseOnClick)
                Close();

            option.Callback?.Invoke();
        }

        private void ReleaseButtons()
        {
            if (_pool == null)
                return;

            foreach (var button in _activeButtons)
            {
                _pool.Push(button);
            }
            _activeButtons.Clear();
        }
    }
}
