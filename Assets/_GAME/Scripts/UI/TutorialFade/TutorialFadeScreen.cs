using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Lobby;
using _GAME.Scripts.UI.Screens.Lobby.Navigation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class TutorialFadeScreen : MonoBehaviour
    {
        [Serializable]
        public class PointerPositionState
        {
            public int Id;
            public EventBus.EventRegion Region;
            public Vector2 Position;
            public Vector2 Size;
            public Vector2 PointerPosition;
            public Vector2 FingerScale;
            public float FingerRotation;
            [Tooltip("Hole follows the lobby navigation button of TargetPlace (Position / Size are ignored, the pointer keeps its offset)")]
            public bool FollowNavigationButton;
            public LobbyPlaceType TargetPlace;
            public Vector2 Padding;
        }

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _holeTransform;
        [SerializeField] private RectTransform _pointerTransform;
        [SerializeField] private RectTransform _fingerTransform;
        [SerializeField] private Button _button;

        [SerializeField] private List<PointerPositionState> _pointerStates;
        
        [Header("Responsive")]
        [SerializeField] private Canvas _canvas; 
        [SerializeField] private RectTransform _rootRect;
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920, 1080);

        private LobbyRoomsController _roomsController;
        private PointerPositionState _lastAppliedState;
        private RectTransform _followTarget;

        private void Awake()
        {
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_rootRect == null) _rootRect = transform as RectTransform;
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;

            EventBus.Subscribe<OpenTutorFadeScreenEvent>(OnShow, EventBus.EventRegion.GLOBAL);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OpenTutorFadeScreenEvent>(OnShow, EventBus.EventRegion.GLOBAL);
        }
        
        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || _lastAppliedState == null) return;
            SetupPosition(_lastAppliedState);
        }

        public void OnShow(OpenTutorFadeScreenEvent e)
        {
            var state = _pointerStates.Find(s => s.Id == e.Id);
            if (state != null)
            {
                SetupPosition(state);
                _button.SetOneShotListener(() =>
                {
                    Hide();
                    PushEvent(e.Id, state.Region);
                });
            }
            Show();
        }

        private void PushEvent(int eId, EventBus.EventRegion stateRegion)
        {
            switch (eId)
            {
                case 0:
                    EventBus.Push(new ToLobbyPlaceEvent(LobbyPlaceType.Bar_Barmen), EventBus.EventRegion.LOBBY);
                    break;
                case 1:
                    EventBus.Push(new ToLobbyPlaceEvent(LobbyPlaceType.Map), EventBus.EventRegion.LOBBY);
                    break;
                case 2:
                    EventBus.Push(new KeyEvent("ToGameplay"), EventBus.EventRegion.GLOBAL);
                    break;
            }
        }

        private void Show()
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1, .5f);
        }

        // the button may still move (panel show animation, layout) - keep the hole over it
        private void LateUpdate()
        {
            if (_followTarget != null && _canvasGroup.blocksRaycasts)
                FollowTarget(_lastAppliedState);
        }

        private void Hide()
        {
            _followTarget = null;
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        }
        
        private (float sx, float sy, float su) GetScale()
        {
            RectTransform targetRoot = _rootRect != null ? _rootRect
                : _canvas != null ? _canvas.transform as RectTransform
                : transform as RectTransform;

            var currentSize = targetRoot != null ? targetRoot.rect.size : new Vector2(Screen.width, Screen.height);
            float sx = _referenceResolution.x > 0 ? currentSize.x / _referenceResolution.x : 1f;
            float sy = _referenceResolution.y > 0 ? currentSize.y / _referenceResolution.y : 1f;
            float su = Mathf.Min(sx, sy);
            return (sx, sy, su);
        }

        private void SetupPosition(PointerPositionState state)
        {
            if (state == null) return;

            _lastAppliedState = state;
            var (sx, sy, su) = GetScale();

            if (_fingerTransform != null)
            {
                _fingerTransform.localScale = state.FingerScale * su;
                _fingerTransform.rotation = Quaternion.Euler(0, 0, state.FingerRotation);
            }

            _followTarget = null;
            if (state.FollowNavigationButton && LobbyNavigationButton.TryGet(state.TargetPlace, out var button))
            {
                _followTarget = button.Rect;
                FollowTarget(state);
                return;
            }

            if (_holeTransform != null)
            {
                _holeTransform.anchoredPosition = new Vector2(state.Position.x * sx, state.Position.y * sy);
                _holeTransform.sizeDelta = new Vector2(state.Size.x * sx, state.Size.y * sy);
            }

            if (_pointerTransform != null)
            {
                _pointerTransform.anchoredPosition = new Vector2(state.PointerPosition.x * sx, state.PointerPosition.y * sy);
            }
        }

        // hole / pointer anchors are bottom-left of the root, positions are in the root space
        private void FollowTarget(PointerPositionState state)
        {
            if (state == null || _followTarget == null || _rootRect == null)
                return;

            var targetCanvas = _followTarget.GetComponentInParent<Canvas>()?.rootCanvas;
            var targetCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? targetCanvas.worldCamera : null;
            var rootCanvas = _canvas != null ? _canvas.rootCanvas : null;
            var rootCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null;

            var corners = new Vector3[4];
            _followTarget.GetWorldCorners(corners);
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var corner in corners)
            {
                var screen = RectTransformUtility.WorldToScreenPoint(targetCamera, corner);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rootRect, screen, rootCamera, out var local);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }

            var (sx, sy, _) = GetScale();
            var origin = _rootRect.rect.min;
            var center = (min + max) * 0.5f - origin;

            if (_holeTransform != null)
            {
                _holeTransform.anchoredPosition = center;
                _holeTransform.sizeDelta = max - min + new Vector2(state.Padding.x * sx, state.Padding.y * sy) * 2f;
            }

            if (_pointerTransform != null)
            {
                var offset = state.PointerPosition - state.Position;
                _pointerTransform.anchoredPosition = center + new Vector2(offset.x * sx, offset.y * sy);
            }
        }

#if UNITY_EDITOR
        [Header("Debug"), SerializeField]
        private int _targetId;

        [ContextMenu("Save")]
        private void Save()
        {
            var state = _pointerStates.Find(s => s.Id == _targetId);
            var targetRegion = EventBus.EventRegion.LOBBY;
            if (state == null)
            {
                state = new PointerPositionState();
                _pointerStates.Add(state);
            }
            else
            {
                targetRegion = state.Region;
            }
            
            var (sx, sy, su) = GetScale();
            float invSx = sx != 0 ? 1f / sx : 1f;
            float invSy = sy != 0 ? 1f / sy : 1f;
            float invSu = su != 0 ? 1f / su : 1f;

            state.Id = _targetId;

            if (_holeTransform != null)
            {
                state.Position = new Vector2(_holeTransform.anchoredPosition.x * invSx, _holeTransform.anchoredPosition.y * invSy);
                state.Size = new Vector2(_holeTransform.sizeDelta.x * invSx, _holeTransform.sizeDelta.y * invSy);
            }

            if (_pointerTransform != null)
            {
                state.PointerPosition = new Vector2(_pointerTransform.anchoredPosition.x * invSx, _pointerTransform.anchoredPosition.y * invSy);
            }

            if (_fingerTransform != null)
            {
                state.FingerScale = _fingerTransform.localScale * invSu;
                state.FingerRotation = _fingerTransform.rotation.eulerAngles.z;
            }

            state.Region = targetRegion;
        }

        [ContextMenu("Load")]
        private void Load()
        {
            var state = _pointerStates.Find(s => s.Id == _targetId);
            if (state != null)
            {
                SetupPosition(state);
            }
            else
            {
                Debug.LogError($"PointerState not found for id {_targetId}");
            }
        }
#endif
    }
}