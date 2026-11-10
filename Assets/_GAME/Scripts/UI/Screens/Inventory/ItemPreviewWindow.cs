using _GAME.Scripts.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class ItemPreviewWindow : ItemInfoWindowBase
    {
        [Header("Placement")]
        [Tooltip("Window body that is moved next to the anchor")]
        [SerializeField] private RectTransform _root;
        [Tooltip("x - gap to the side (towards the screen center), y - how much lower than the anchor center the top edge is")]
        [SerializeField] private Vector2 _offset = new(20f, 40f);
        [Tooltip("Min distance from the screen (parent) edges")]
        [SerializeField, Min(0f)] private float _screenMargin = 20f;

        private Vector2? _rootDefaultPosition;

        public static void Open(ItemInfo item, RectTransform anchor = null)
        {
            var request = new ItemInfoRequest(item)
            {
                StatsMode = EquipmentStatsMode.Current,
                AllowPreview = false,
                Anchor = anchor,
            };
            Core.Get<UIManager>().OpenWindow<ItemPreviewWindow>().Setup(request);
        }

        protected override RectTransform DefaultClickArea => _root != null ? _root : base.DefaultClickArea;

        protected override void AlignToAnchor(RectTransform anchor)
        {
            AlignRoot(anchor);
            AlignArrow(anchor);
        }

        // Root goes slightly below the anchor, on its side facing the screen center, clamped inside the parent
        private void AlignRoot(RectTransform anchor)
        {
            if (_root == null || _root.parent is not RectTransform parent)
                return;

            _rootDefaultPosition ??= _root.anchoredPosition;
            _root.anchoredPosition = _rootDefaultPosition.Value;
            if (anchor == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
            var parentRect = parent.rect;
            var rect = _root.rect;
            var anchorCenter = (Vector2)parent.InverseTransformPoint(GetWorldCenter(anchor));
            var anchorHalfWidth = anchor.rect.width * 0.5f * anchor.lossyScale.x / parent.lossyScale.x;

            var toRight = IsOnLeftOfCanvas(anchor);
            var sideX = anchorCenter.x + (toRight ? anchorHalfWidth + _offset.x : -anchorHalfWidth - _offset.x);
            var x = toRight ? sideX - rect.xMin : sideX - rect.xMax;
            var y = anchorCenter.y - _offset.y - rect.yMax;

            x = Mathf.Clamp(x, parentRect.xMin + _screenMargin - rect.xMin, parentRect.xMax - _screenMargin - rect.xMax);
            y = Mathf.Clamp(y, parentRect.yMin + _screenMargin - rect.yMin, parentRect.yMax - _screenMargin - rect.yMax);
            _root.localPosition = new Vector3(x, y, _root.localPosition.z);
        }
    }
}
