using System;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens
{
    public class LoadingScreen : UIWindow
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _blocker;
        private async void Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            _canvasGroup.DOFade(0, .5f);
            EventBus.Subscribe<SceneLoadEvent>(OnShow, EventBus.EventRegion.GAMEPLAY);
        }
        
        public void OnShow(SceneLoadEvent keyEvent)
        {
            _blocker.raycastTarget = true;
            _canvasGroup.DOFade(1, .5f);
        }
        
        public void OnHide(SceneLoadCompleteEvent keyEvent)
        {
            
            _canvasGroup.DOFade(0, .5f).OnComplete(()=>_blocker.raycastTarget = false);
        }
    }
}