using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Events;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UIAimTutorial : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private Player _player;
        
        public void Setup(Player player)
        {
            _player = player;
            _canvasGroup.alpha = 0;
            EventBus.Subscribe<ShowAimTutorialEvent>(Show, EventBus.EventRegion.GAMEPLAY);
        }

        private void Show(ShowAimTutorialEvent tutorialEvent)
        {
            EventBus.Unsubscribe<ShowAimTutorialEvent>(Show, EventBus.EventRegion.GAMEPLAY);
            _canvasGroup.DOFade(1, .2f);
            _player.OnShot += PlayerOnOnShot;
            
        }

        private void PlayerOnOnShot()
        {
            _canvasGroup.DOFade(0, .2f);
            _player.OnShot -= PlayerOnOnShot;
            EventBus.Push(new KeyEvent("HideAimTutorial"), EventBus.EventRegion.GAMEPLAY);
        }
    }
}