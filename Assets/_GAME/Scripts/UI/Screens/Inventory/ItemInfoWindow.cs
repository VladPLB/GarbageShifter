using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class ItemInfoWindow : ItemInfoWindowBase
    {
        [Header("Buttons")]
        [SerializeField] private RectTransform _buttonsHolder;
        [SerializeField] private ItemInfoButton _buttonPrefab;

        [Header("Placement")]
        [Tooltip("Window body that is shifted from its default (centered) position")]
        [SerializeField] private RectTransform _root;
        [Tooltip("Gap between the canvas center and the near edge of the root")]
        [SerializeField, Min(0f)] private float _rootSideOffset = 50f;
        [Tooltip("true - root goes to the half of the screen opposite to the anchor (doesn't cover it), false - to the anchor half")]
        [SerializeField] private bool _shiftOppositeToAnchor = true;

        private Vector2? _rootDefaultPosition;

        private LocalPool<ItemInfoButton> _pool;
        private readonly List<ItemInfoButton> _activeButtons = new();

        public ItemInfo Item => Request?.Item;

        public override void OnOpen()
        {
            _pool ??= new LocalPool<ItemInfoButton>(_buttonPrefab);
            base.OnOpen();
        }

        public override void OnClose()
        {
            ClosePreview();
            ReleaseButtons();
            base.OnClose();
        }

        public override void Setup(ItemInfoRequest request)
        {
            ClosePreview();
            base.Setup(request);
            SetupButtons(request);
        }

        protected override RectTransform DefaultClickArea => _root != null ? _root : base.DefaultClickArea;

        protected override void AlignToAnchor(RectTransform anchor)
        {
            ShiftRoot(anchor);
            AlignArrow(anchor);
        }

        // Root is placed on the left or right half of the canvas (by the anchor side), its near edge _rootSideOffset from the center;
        // works with any root pivot, vertical position stays as in the prefab
        private void ShiftRoot(RectTransform anchor)
        {
            if (_root == null || _root.parent is not RectTransform parent)
                return;

            _rootDefaultPosition ??= _root.anchoredPosition;
            _root.anchoredPosition = _rootDefaultPosition.Value;
            var canvasRect = RootCanvasRect;
            if (anchor == null || canvasRect == null)
                return;

            var toRight = IsOnLeftOfCanvas(anchor) == _shiftOppositeToAnchor;
            var centerWorld = canvasRect.TransformPoint(canvasRect.rect.center);
            var centerX = parent.InverseTransformPoint(centerWorld).x;
            var rect = _root.rect;
            var x = toRight ? centerX + _rootSideOffset - rect.xMin : centerX - _rootSideOffset - rect.xMax;
            _root.localPosition = new Vector3(x, _root.localPosition.y, _root.localPosition.z);
        }

        private void SetupButtons(ItemInfoRequest request)
        {
            ReleaseButtons();

            foreach (var option in request.Buttons)
            {
                var button = _pool.Pop();
                button.Setup(option.View, option.Interactable, () => OnButtonClicked(option));
                button.transform.SetParent(_buttonsHolder, false);
                button.transform.localScale = Vector3.one;
                button.gameObject.SetActive(true);
                _activeButtons.Add(button);
            }
        }

        private void OnButtonClicked(ItemInfoButtonOption option)
        {
            if (option.CloseOnClick)
                Close();

            option.Callback?.Invoke();
        }

        private void ClosePreview()
        {
            if (manager != null)
                manager.GetWindow<ItemPreviewWindow>()?.Close();
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
