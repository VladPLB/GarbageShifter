using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Communications
{
    public class UIGameCommunicator : MonoBehaviour, IReparentIgnored
    {
        private static readonly int ShowKey = Animator.StringToHash("Show");
        private static readonly int HideKey = Animator.StringToHash("Hide");
        private static readonly TimeSpan ShowedTime = TimeSpan.FromSeconds(1f);
        private static readonly TimeSpan HideTime = TimeSpan.FromSeconds(.5f);
        
        [SerializeField] private Animator _animator;
        [SerializeField] private Image _portrait;
        [SerializeField] private SubtitlesQueueViewer _subtitles;

        private bool _showed = false;
        private List<string> _messages = new();

        public event Action OnCompleted;
        
        public void Setup()
        {
            EventBus.Subscribe<CommunicatorMessageEvent>(ShowMessage, EventBus.EventRegion.GAMEPLAY);
            _subtitles.Setup();
        }
        
        public void ShowMessage(string message)
        {
            _messages.Add(message);
            Show();
        }

        public void ShowMessage(List<string> messages)
        {
            _messages.AddRange(messages);
            Show();
        }
        
        private void ShowMessage(CommunicatorMessageEvent messageEvent)
        {
            _messages.AddRange(messageEvent.Messages);
            Show();
        }
        
        private async void Show()
        {
            if (!_showed)
            {
                _animator.SetTrigger(ShowKey);
            }
            await UniTask.Delay(ShowedTime);
            _showed = true;
            _subtitles.OnEnd += CompleteCommunication;
            _subtitles.ShowMessages(_messages);
            _messages.Clear();
        }

        private async void CompleteCommunication()
        {
            if (_showed)
            {
                _animator.SetTrigger(HideKey);
            }
            
            await UniTask.Delay(HideTime);
            _showed = false;
            OnCompleted?.Invoke();
            OnCompleted = null;
            EventBus.Push(new CommunicatorCompleteEvent(), EventBus.EventRegion.GAMEPLAY);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CommunicatorMessageEvent>(ShowMessage, EventBus.EventRegion.GAMEPLAY);
        }
    }
}