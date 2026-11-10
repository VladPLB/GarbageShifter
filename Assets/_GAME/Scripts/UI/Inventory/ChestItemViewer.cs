using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Inventory.UsePipelines;
using _GAME.Scripts.Pools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Inventory
{
    // Store (chests / offers) item: icon, name, price, "Best" badge and the timer to the next free daily chest
    public class ChestItemViewer : MonoBehaviour, IPoolableItem
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private List<Color> _rangeColors;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private Button _button;

        [Header("Price")]
        [Tooltip("Price, or the timer to the next free chest when the daily chest is already claimed")]
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private string _freeText = "Free";

        [Header("Badges")]
        [SerializeField] private GameObject _bestMark;

        private ItemInfo _item;
        private InventoryManager _inventoryManager;
        private bool _isDaily;
        private bool _isWaitingDaily;
        private float _nextTimerUpdate;
        private string _price;

        public ItemInfo Item => _item;

        // raised when the daily chest becomes available again (the window may refresh the list)
        public event Action<ChestItemViewer> OnDailyAvailable;

        public void Setup(ItemInfo item, Action clickHandler)
        {
            _item = item;
            _inventoryManager ??= Core.Get<InventoryManager>();

            _iconImage.sprite = item.Icon;
            if (_backgroundImage != null && _rangeColors.Count > 0)
                _backgroundImage.color = _rangeColors[Mathf.Min((int)item.Rank, _rangeColors.Count - 1)];
            if (_nameLabel != null)
                _nameLabel.text = item.DisplayName;
            if (_button != null)
                _button.SetListener(clickHandler);

            SetBest(false);
            SetPrice(GetRecipePrice(item));

            _isDaily = item.UsePipelines.OfType<ItemUsePipelineDailyRewardChest>().Any();
            _nextTimerUpdate = 0f;
            UpdateDailyState();
        }

        // null / empty - price is hidden (e.g. store prices are not loaded yet); while the daily chest is claimed the label shows the timer
        public void SetPrice(string price)
        {
            _price = price;
            RefreshPriceLabel();
        }

        public void SetBest(bool isBest)
        {
            if (_bestMark != null)
                _bestMark.SetActive(isBest);
        }

        private void Update()
        {
            if (!_isWaitingDaily || Time.unscaledTime < _nextTimerUpdate)
                return;

            _nextTimerUpdate = Time.unscaledTime + 1f;
            UpdateDailyState();
            if (!_isWaitingDaily)
                OnDailyAvailable?.Invoke(this);
        }

        private void UpdateDailyState()
        {
            _isWaitingDaily = _isDaily && _inventoryManager != null && !_inventoryManager.IsDailyClaimAvailable(_item.Id);
            RefreshPriceLabel();
        }

        // the same label shows the price or the timer to the next free chest
        private void RefreshPriceLabel()
        {
            if (_priceText == null)
                return;

            if (_isWaitingDaily)
            {
                var time = InventoryManager.TimeToNextDay;
                _priceText.text = $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
                _priceText.gameObject.SetActive(true);
                return;
            }

            _priceText.text = _price ?? string.Empty;
            _priceText.gameObject.SetActive(!string.IsNullOrEmpty(_price));
        }

        private string GetRecipePrice(ItemInfo item)
        {
            var costs = item.Recipe?.ItemCosts;
            if (costs == null || costs.Count == 0 || costs[0].Item == null)
                return _freeText;

            return $"{costs[0].Item.DisplayName}x{costs[0].Amount}";
        }

        private void OnDisable()
        {
            OnDailyAvailable = null;
        }
    }
}
