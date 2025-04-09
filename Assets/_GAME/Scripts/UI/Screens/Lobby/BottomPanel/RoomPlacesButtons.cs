using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Lobby
{
    public class RoomPlacesButtons : MonoBehaviour
    {
        [SerializeField] private RectTransform _holderTransform;
        [SerializeField] private Vector2 _hidePosition;
        [SerializeField] private Vector2 _showPosition;
        [SerializeField] 
        private List<BottomPanelPlaceButton> _buttons;

        private List<LobbyCameraType> _types;
        private bool _isAnimate = false;
        private Action<LobbyCameraType> _onClick;
        
        public void Setup()
        {
            _holderTransform.anchoredPosition = _hidePosition;
        }
        
        public async void Show(List<LobbyCameraType> types, LobbyCameraType targetType, Action<LobbyCameraType> onClick)
        {
            if(_isAnimate)
                return;
            
            _isAnimate = true;
            float showDelay = .2f;
            if (_types == null || _types[0] != types[0])
            {
               /* if (_types != null)
                {
                    await Hide(true);
                    showDelay = 2.6f;
                }
                */
                _types = types;
                for (int i = 0; i < _buttons.Count; i++)
                {
                    _buttons[i].Setup(i < _types.Count?_types[i]: LobbyCameraType.Transition, OnClick);
                }
                if(types.Count>1)
                {
                    _onClick = onClick;
                    Select(types.Contains(targetType) ? targetType : types[0]);
                    await UniTask.Delay(TimeSpan.FromSeconds(showDelay));
                    await _holderTransform.DOAnchorPos(_showPosition, .2f).AsyncWaitForCompletion();
                }
            }
            _isAnimate = false;
        }

        public async UniTask Hide(bool force = false)
        {
            if(!force)
            {
                if (_isAnimate)
                    return;
                _isAnimate = true;
            }
            await _holderTransform.DOAnchorPos(_hidePosition, .2f).AsyncWaitForCompletion();
            if(!force)
            {
                _isAnimate = false;
            }
        }
        
        public async void TryHide(LobbyCameraType targetType)
        {
            if (_types != null && !_types.Contains(targetType))
            {
                await Hide();
            }
        }

        public void Select(LobbyCameraType type)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].Select(type);
            }
        }

        private void OnClick(LobbyCameraType type)
        {
            _onClick?.Invoke(type);
        }

        
    }
}