using System;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        private void Awake()
        {
            EventBus.Subscribe<SceneLoadEvent>(OnShow, EventBus.EventRegion.GLOBAL);
            EventBus.Subscribe<SceneLoadCompleteEvent>(OnHide, EventBus.EventRegion.GLOBAL);
            
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
        }
        
        public void OnShow(SceneLoadEvent keyEvent)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1, .5f);
        }
        
        public void OnHide(SceneLoadCompleteEvent keyEvent)
        {
            
            _canvasGroup.DOFade(0, .2f).OnComplete(()=>_canvasGroup.blocksRaycasts = false);
        }
    }
}