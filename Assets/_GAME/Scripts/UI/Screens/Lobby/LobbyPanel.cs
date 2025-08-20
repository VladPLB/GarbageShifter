using System;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class LobbyPanel: MonoBehaviour
    {
        [SerializeField] protected string _panelName;
        [SerializeField] protected RectTransform _holderTransform;
        [SerializeField] protected Vector2 _hidePosition;
        [SerializeField] protected Vector2 _showPosition;

        private void Awake()
        {
            EventBus.Subscribe<SetEnableLobbyPanelEvent>(OnEnableLobbyPanelEventHandler, EventBus.EventRegion.LOBBY);
        }

        private void OnEnableLobbyPanelEventHandler(SetEnableLobbyPanelEvent e)
        {
            if (e.PanelName == _panelName)
            {
                if (e.IsEnable)
                    Show().Forget();
                else
                    Hide().Forget();
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SetEnableLobbyPanelEvent>(OnEnableLobbyPanelEventHandler, EventBus.EventRegion.LOBBY);
        }

        public virtual async UniTask Show()
        {
            _holderTransform.DOKill();
            await _holderTransform.DOAnchorPos(_showPosition, .2f).AsyncWaitForCompletion();
        }

        public virtual async UniTask Hide()
        {
            _holderTransform.DOKill();
            await _holderTransform.DOAnchorPos(_hidePosition, .2f).AsyncWaitForCompletion();
        }
    }
}