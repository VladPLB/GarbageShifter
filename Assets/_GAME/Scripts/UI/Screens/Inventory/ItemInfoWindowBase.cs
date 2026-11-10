using System.Collections.Generic;
using _GAME.Scripts.UI.Screens.Inventory.InfoSections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [Header("Close on click outside")]
        [Tooltip("Press outside the window closes it and the click goes further (another item opens at once). The full screen close button stops catching clicks")]
        [SerializeField] private bool _closeOnClickOutside = true;
        [Tooltip("Window area; empty - the placement root (or the whole window)")]
        [SerializeField] private RectTransform _clickArea;

        // open info windows in opening order: a click inside a later (child) window doesn't close earlier ones
        private static readonly List<ItemInfoWindowBase> OpenWindows = new();

        [Header("Anchor arrow")]
        [Tooltip("Arrow pointing to the element the window was opened from; slides along its edge and rotates to it")]
        [SerializeField] protected RectTransform _arrow;
        [Tooltip("Arrow distance from the ends of its edge")]
        [SerializeField, Min(0f)] private float _arrowEdgeMargin = 30f;
        [SerializeField] private bool _rotateArrow = true;

        private Vector2? _arrowDefaultPosition;
        private Quaternion _arrowDefaultRotation;

        protected ItemInfoRequest Request { get; private set; }

        public override void OnOpen()
        {
            if (_closeButton != null)
            {
                _closeButton.SetListener(Close);
                if (_closeOnClickOutside)
                    SetRaycastTarget(_closeButton.gameObject, false);
            }

            OpenWindows.Remove(this);
            OpenWindows.Add(this);
            base.OnOpen();
        }

        public override void OnClose()
        {
            OpenWindows.Remove(this);
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

            AlignToAnchor(request.Anchor);
        }

        private RectTransform ClickArea => _clickArea != null ? _clickArea : DefaultClickArea;
        protected virtual RectTransform DefaultClickArea => (RectTransform)transform;

        protected virtual void Update()
        {
            if (!_closeOnClickOutside)
                return;

            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            var position = pointer.position.ReadValue();
            var index = OpenWindows.IndexOf(this);
            for (int i = Mathf.Max(0, index); i < OpenWindows.Count; i++)
            {
                if (OpenWindows[i].ContainsScreenPoint(position))
                    return;
            }
            Close();
        }

        private bool ContainsScreenPoint(Vector2 screenPoint)
        {
            var area = ClickArea;
            if (area == null)
                return false;

            var canvas = GetComponentInParent<Canvas>();
            var camera = canvas == null || canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
            return RectTransformUtility.RectangleContainsScreenPoint(area, screenPoint, camera);
        }

        private static void SetRaycastTarget(GameObject target, bool isEnabled)
        {
            foreach (var graphic in target.GetComponents<Graphic>())
                graphic.raycastTarget = isEnabled;
        }

        protected virtual void AlignToAnchor(RectTransform anchor)
        {
            AlignArrow(anchor);
        }

        // Arrow keeps its edge (designer placed) and slides / rotates to point at the anchor; no anchor - default placement
        protected void AlignArrow(RectTransform anchor)
        {
            if (_arrow == null || _arrow.parent is not RectTransform arrowParent)
                return;

            if (_arrowDefaultPosition == null)
            {
                _arrowDefaultPosition = _arrow.anchoredPosition;
                _arrowDefaultRotation = _arrow.localRotation;
            }

            _arrow.anchoredPosition = _arrowDefaultPosition.Value;
            _arrow.localRotation = _arrowDefaultRotation;
            if (anchor == null)
                return;

            Canvas.ForceUpdateCanvases();
            var target = (Vector2)arrowParent.InverseTransformPoint(GetWorldCenter(anchor));
            var rect = arrowParent.rect;
            var local = (Vector2)_arrow.localPosition;

            // slide along the edge the arrow is placed on (top / bottom edge - by x, side edge - by y)
            var isHorizontalEdge = Mathf.Abs(local.y - rect.center.y) / rect.height >= Mathf.Abs(local.x - rect.center.x) / rect.width;
            if (isHorizontalEdge)
                local.x = target.x;
            else
                local.y = target.y;
            _arrow.localPosition = ClampInsideParent(local, rect, isHorizontalEdge);

            if (_rotateArrow)
            {
                // arrow sprite points up (local Y)
                var direction = target - local;
                if (direction.sqrMagnitude > 0.01f)
                    _arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            }
        }

        // The whole arrow (any rotation) stays inside its parent; the sliding axis also keeps the edge margin
        private Vector3 ClampInsideParent(Vector2 local, Rect parentRect, bool isHorizontalEdge)
        {
            var size = Vector2.Scale(_arrow.rect.size, _arrow.localScale);
            var half = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y)) * 0.5f;
            var marginX = isHorizontalEdge ? Mathf.Max(half, _arrowEdgeMargin) : half;
            var marginY = isHorizontalEdge ? half : Mathf.Max(half, _arrowEdgeMargin);

            local.x = ClampOrCenter(local.x, parentRect.xMin + marginX, parentRect.xMax - marginX);
            local.y = ClampOrCenter(local.y, parentRect.yMin + marginY, parentRect.yMax - marginY);
            return new Vector3(local.x, local.y, _arrow.localPosition.z);
        }

        // parent smaller than the arrow: keep it centered
        private static float ClampOrCenter(float value, float min, float max)
        {
            return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * 0.5f;
        }

        // Side of the screen is taken from the root canvas, not from the window's parent
        protected bool IsOnLeftOfCanvas(RectTransform anchor)
        {
            var canvasRect = RootCanvasRect;
            if (canvasRect == null)
                return true;

            var x = canvasRect.InverseTransformPoint(GetWorldCenter(anchor)).x;
            return x <= canvasRect.rect.center.x;
        }

        protected RectTransform RootCanvasRect
        {
            get
            {
                var canvas = GetComponentInParent<Canvas>();
                return canvas != null ? (RectTransform)canvas.rootCanvas.transform : null;
            }
        }

        protected static Vector3 GetWorldCenter(RectTransform rectTransform)
        {
            return rectTransform.TransformPoint(rectTransform.rect.center);
        }
    }
}
