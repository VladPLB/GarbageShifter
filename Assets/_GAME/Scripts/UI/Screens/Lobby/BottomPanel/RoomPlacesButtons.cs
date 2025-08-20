using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class RoomPlacesButtons : LobbyPanel
    {
        [SerializeField] 
        private List<BottomPanelPlaceButton> _buttons;

        private List<LobbyPlaceType> _types;
        private bool _isAnimate = false;
        private Action<LobbyPlaceType> _onClick;
        
        public void Setup()
        {
            _holderTransform.anchoredPosition = _hidePosition;
        }
        
        public async void Show(List<LobbyPlaceType> types, LobbyPlaceType targetType, Action<LobbyPlaceType> onClick)
        {
            if(_isAnimate)
                return;
            
            _isAnimate = true;
            float showDelay = .2f;
            if (_types == null || _types[0] != types[0])
            {
                _types = types;
                for (int i = 0; i < _buttons.Count; i++)
                {
                    _buttons[i].Setup(i < _types.Count?_types[i]: LobbyPlaceType.Transition, OnClick);
                }
                if(types.Count>1)
                {
                    _onClick = onClick;
                    Select(types.Contains(targetType) ? targetType : types[0]);
                    await UniTask.Delay(TimeSpan.FromSeconds(showDelay));
                    await Show();
                }
            }
            _isAnimate = false;
        }

        public async UniTask Hide(bool force)
        {
            if(!force)
            {
                if (_isAnimate)
                    return;
                _isAnimate = true;
            }

            await Hide();
            if(!force)
            {
                _isAnimate = false;
            }
        }
        
        public async void TryHide(LobbyPlaceType targetType)
        {
            if (_types != null && !_types.Contains(targetType))
            {
                await Hide(false);
            }
        }

        public void Select(LobbyPlaceType type)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].Select(type);
            }
        }

        private void OnClick(LobbyPlaceType type)
        {
            _onClick?.Invoke(type);
        }

        
    }
}