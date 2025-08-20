using System;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens
{
    public class BlackFadeScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        private void Awake()
        {
            EventBus.Subscribe<ShowBlackFadeEvent>(OnShow, EventBus.EventRegion.GLOBAL);
        }
        
        public void OnShow(ShowBlackFadeEvent keyEvent)
        {
            if(keyEvent.IsShow)
            {
                _canvasGroup.blocksRaycasts = true;
                if (keyEvent.Duration <= 0)
                {
                    _canvasGroup.alpha = 1;
                }
                else
                {
                    _canvasGroup.DOFade(1, keyEvent.Duration);
                }
            }
            else
            {
                if (keyEvent.Duration <= 0)
                {
                    _canvasGroup.alpha = 0;
                    _canvasGroup.blocksRaycasts = false;
                }
                else
                {
                    _canvasGroup.DOFade(0, keyEvent.Duration).OnComplete(()=>_canvasGroup.blocksRaycasts = false);
                }
            }
        }
    }
}