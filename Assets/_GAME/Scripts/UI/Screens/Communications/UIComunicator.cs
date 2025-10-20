using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Communications
{
    public class UIComunicator : MonoBehaviour, IReparentIgnored
    {
        private static readonly int ShowKey = Animator.StringToHash("Show");
        private static readonly int HideKey = Animator.StringToHash("Hide");
        private static readonly TimeSpan ShowedTime = TimeSpan.FromSeconds(1f);
        private static readonly TimeSpan HideTime = TimeSpan.FromSeconds(.5f);

        [SerializeField] private EventBus.EventRegion _region;
        [SerializeField] private Animator _animator;
        [SerializeField] private Image _portrait;
        [SerializeField] private SubtitlesQueueViewer _subtitles;

        private bool _showed = false;
        private List<string> _messages = new();

        public event Action OnCompleted;
        
        public void Setup()
        {
            EventBus.Subscribe<CommunicatorMessageEvent>(ShowMessage, _region);
            _subtitles.Setup();
        }

        public void ShowMessage(List<string> messages)
        {
            _messages.AddRange(messages);
            Show();
        }
        
        private void ShowMessage(CommunicatorMessageEvent messageEvent)
        {
            ShowMessage(messageEvent.Messages);
        }
        
        private async void Show()
        {
            EventBus.Push(new KeyEvent("LockUI"), EventBus.EventRegion.GLOBAL);
            if (!_showed)
            {
                _animator.SetTrigger(ShowKey);
            }
            await UniTask.Delay(ShowedTime);
            _showed = true;
            _subtitles.OnEnd += CompleteDialog;
            _subtitles.ShowMessages(_messages);
            _messages.Clear();
        }

        private async void CompleteDialog()
        {
            if (_showed)
            {
                _animator.SetTrigger(HideKey);
            }
            
            await UniTask.Delay(HideTime);
            EventBus.Push(new KeyEvent("UnlockUI"), EventBus.EventRegion.GLOBAL);
            _showed = false;
            OnCompleted?.Invoke();
            OnCompleted = null;
            EventBus.Push(new DialogCompleteEvent(), _region);
            
        }

        public void OnDestroy()
        {
            EventBus.Unsubscribe<CommunicatorMessageEvent>(ShowMessage, _region);
        }
    }
}