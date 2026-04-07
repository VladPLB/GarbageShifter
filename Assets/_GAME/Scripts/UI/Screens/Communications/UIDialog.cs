using System;
using System.Collections.Generic;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Communications
{
    public class UIDialog : UIWindow, IReparentIgnored
    {
        public enum PositionType
        {
            Middle,
            Bottom,
            Top
        }

        [Serializable]
        public struct PanelPosition
        {
            public PositionType Type;
            public Vector2 RectPosition;
        }
        
        private static readonly int ShowKey = Animator.StringToHash("Show");
        private static readonly int HideKey = Animator.StringToHash("Hide");
        private static readonly TimeSpan ShowedTime = TimeSpan.FromSeconds(1f);
        private static readonly TimeSpan HideTime = TimeSpan.FromSeconds(.5f);

        [SerializeField] private EventBus.EventRegion _region;
        [SerializeField] private Animator _animator;
        [SerializeField] private SubtitlesQueueViewer _subtitles;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private RectTransform _panelRoot;
        [SerializeField] private List<PanelPosition> _positions;

        private bool _showed = false;
        private NPCName _name;
        private List<string> _messages = new();
        private List<NPCAnimationType> _animations = new();
        private int _currentAnimation = 0;

        public event Action OnCompleted;

        public override void OnOpen()
        {
            base.OnOpen();
            EventBus.Subscribe<DialogMessageEvent>(ShowMessage, _region);
            _subtitles.Setup();
        }

        public void SetPosition(PositionType type)
        {
            foreach (var position in _positions)
            {
                if (position.Type == type)
                {
                    _panelRoot.anchoredPosition = position.RectPosition;
                    return;
                }
            }
            _panelRoot.anchoredPosition = _positions[0].RectPosition;
        }

        public void ShowMessage(NPCName npcName, List<string> messages, List<NPCAnimationType> animations)
        {
            _name = npcName;
            _messages.AddRange(messages);
            _animations.AddRange(animations);
            Show();
        }
        
        private void ShowMessage(DialogMessageEvent messageEvent)
        {
            ShowMessage(messageEvent.Name,messageEvent.Messages, messageEvent.Animations);
        }
        
        private async void Show()
        {
            EventBus.Push(new KeyEvent("LockUI"), EventBus.EventRegion.GLOBAL);
            _nameLabel.text = _name.ToString();
            if (!_showed)
            {
                EventBus.Push(new SoundPlayEvent(SoundType.ShowCommunicator, null),  EventBus.EventRegion.GLOBAL);
                _animator.SetTrigger(ShowKey);
            }
            await UniTask.Delay(ShowedTime);
            _showed = true;
            _currentAnimation = 0;
            _subtitles.OnNextMessage += NextAnimation;
            _subtitles.OnEnd += OnEnd;
            
            _subtitles.ShowMessages(_messages);
            _messages.Clear();
        }

        private void OnEnd()
        {
            _animations.Clear();
            OnCompleted?.Invoke();
            OnCompleted = null;
            EventBus.Push(new DialogCompleteEvent(), _region);
        }

        private void NextAnimation()
        {
            if(_currentAnimation < _animations.Count && _animations[_currentAnimation] != NPCAnimationType.None)
            {
                NpcAnimationEvent animationEvent = new NpcAnimationEvent(_name, _animations[_currentAnimation]);
                EventBus.Push(animationEvent, _region);
            }
            _currentAnimation++;
        }

        private async UniTask CompleteDialog()
        {
            if (_showed)
            {
                _animator.SetTrigger(HideKey);
                EventBus.Push(new SoundPlayEvent(SoundType.HideCommunicator, null),  EventBus.EventRegion.GLOBAL);
            }
            
            await UniTask.Delay(HideTime);
            _showed = false;
            EventBus.Push(new KeyEvent("UnlockUI"), EventBus.EventRegion.GLOBAL);
        }

        public override async void OnClose()
        {
            await CompleteDialog();
            EventBus.Unsubscribe<DialogMessageEvent>(ShowMessage, _region);
            base.OnClose();
        }
    }
}