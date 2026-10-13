using _GAME.Scripts.Inventory;
using _GAME.Scripts.Pools;
using _GAME.Scripts.UI.Screens.Inventory;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Lobby.Currency
{
    public class CurrencyViewer : MonoBehaviour, IPoolableItem
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountLabel;
        [SerializeField] private Button _button;

        [Header("Reaction")]
        [SerializeField] private RectTransform _punchTarget;
        [SerializeField] private TextMeshProUGUI _deltaLabel;
        [SerializeField] private Color _gainColor = Color.green;
        [SerializeField] private Color _spendColor = Color.red;
        [SerializeField] private float _countDuration = 0.4f;
        [SerializeField] private float _punchScale = 0.15f;
        [SerializeField] private float _punchDuration = 0.3f;
        [SerializeField] private float _deltaRise = 40f;
        [SerializeField] private float _deltaDuration = 0.8f;

        private int _displayedAmount;
        private int _targetAmount;
        private Vector2 _deltaStartPosition;
        private bool _deltaPositionCached;
        private Tween _countTween;
        private Sequence _deltaSequence;

        public ItemInfo Item { get; private set; }

        public void Setup(ItemInfo item, int amount)
        {
            Item = item;
            _iconImage.sprite = item.Icon;

            if (_button != null)
                _button.SetListener(() => ItemPreviewWindow.Open(Item));

            KillTweens();
            _displayedAmount = amount;
            _targetAmount = amount;
            UpdateAmountLabel();
            HideDelta();
        }

        public void ChangeAmount(int amount)
        {
            var delta = amount - _targetAmount;
            if (delta == 0)
                return;

            _targetAmount = amount;

            _countTween?.Kill();
            _countTween = DOTween.To(() => _displayedAmount, SetDisplayedAmount, amount, _countDuration)
                .SetEase(Ease.OutCubic);

            if (_punchTarget != null)
            {
                _punchTarget.DOKill(true);
                _punchTarget.DOPunchScale(Vector3.one * _punchScale, _punchDuration);
            }

            ShowDelta(delta);
        }

        private void SetDisplayedAmount(int amount)
        {
            _displayedAmount = amount;
            UpdateAmountLabel();
        }

        private void UpdateAmountLabel()
        {
            _amountLabel.text = _displayedAmount.ToString();
        }

        private void ShowDelta(int delta)
        {
            if (_deltaLabel == null)
                return;

            CacheDeltaPosition();
            _deltaSequence?.Kill();

            var rect = _deltaLabel.rectTransform;
            rect.anchoredPosition = _deltaStartPosition;
            _deltaLabel.text = delta > 0 ? $"+{delta}" : delta.ToString();
            _deltaLabel.color = delta > 0 ? _gainColor : _spendColor;
            _deltaLabel.gameObject.SetActive(true);

            _deltaSequence = DOTween.Sequence()
                .Join(rect.DOAnchorPosY(_deltaStartPosition.y + _deltaRise, _deltaDuration))
                .Join(_deltaLabel.DOFade(0f, _deltaDuration).SetEase(Ease.InQuad))
                .OnComplete(HideDelta);
        }

        private void HideDelta()
        {
            if (_deltaLabel == null)
                return;

            CacheDeltaPosition();
            _deltaLabel.rectTransform.anchoredPosition = _deltaStartPosition;
            _deltaLabel.gameObject.SetActive(false);
        }

        private void CacheDeltaPosition()
        {
            if (_deltaPositionCached)
                return;

            _deltaStartPosition = _deltaLabel.rectTransform.anchoredPosition;
            _deltaPositionCached = true;
        }

        private void KillTweens()
        {
            _countTween?.Kill();
            _deltaSequence?.Kill();
            if (_punchTarget != null)
                _punchTarget.DOKill(true);
        }

        private void OnDisable()
        {
            KillTweens();
            SetDisplayedAmount(_targetAmount);
            HideDelta();
        }
    }
}
