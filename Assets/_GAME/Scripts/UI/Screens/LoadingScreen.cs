using System;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        private async void Start()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            _canvasGroup.DOFade(0, .5f);
            EventBus.Subscribe<KeyEvent>(OnEvent, EventBus.EventRegion.GAMEPLAY);
        }
        
        public void OnEvent(KeyEvent keyEvent)
        {
            if(keyEvent.Key == "ShowLoading")
            {
                EventBus.Unsubscribe<KeyEvent>(OnEvent, EventBus.EventRegion.GAMEPLAY);
                _canvasGroup.DOFade(1, .5f);
            }
        }
    }
}