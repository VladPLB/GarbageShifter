using System;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UIGameAttention : MonoBehaviour
    {
        private static readonly int ActiveKey = Animator.StringToHash("Active");
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _atentionDuration;
        [SerializeField] private float _afterHideDuration;
        [SerializeField] private TextMeshProUGUI _textLabel;
       

        public void Setup()
        {
            _canvasGroup.alpha = 0;
            EventBus.Subscribe<AttentionEvent>(Show, EventBus.EventRegion.GAMEPLAY);
        }

        private async void Show(AttentionEvent attentionEvent)
        {
            var ct = this.GetCancellationTokenOnDestroy();
            try
            {
                _textLabel.text = attentionEvent.Text;
                _canvasGroup.alpha = 0;
                _animator.Rebind();
                _canvasGroup.DOFade(1, .2f).OnComplete( ()=>
                {
                    _animator.SetBool(ActiveKey, true);
                    AudioManager.Play(SoundType.Notification);
                });

                await UniTask.Delay(TimeSpan.FromSeconds(_atentionDuration), cancellationToken: ct);
                _animator.SetBool(ActiveKey, false);
                await UniTask.Delay(TimeSpan.FromSeconds(_afterHideDuration), cancellationToken: ct);
                _canvasGroup.DOFade(0, .2f);
            }
            catch (OperationCanceledException)
            {
                // component destroyed mid-await, expected
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AttentionEvent>(Show, EventBus.EventRegion.GAMEPLAY);
        }
    }
}