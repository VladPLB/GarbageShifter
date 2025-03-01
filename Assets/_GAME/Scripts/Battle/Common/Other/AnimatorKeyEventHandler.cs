using System;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Common.Other
{
    public class AnimatorKeyEventHandler: MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _animatorKey = "Play";
        [SerializeField] private string _eventKey = "Play";

        private void OnEnable()
        {
            EventBus.Subscribe<KeyEvent>(EventListener, EventBus.EventRegion.GAMEPLAY);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe<KeyEvent>(EventListener, EventBus.EventRegion.GAMEPLAY);
        }

        private void EventListener(KeyEvent keyEvent)
        {
            if(keyEvent.Key == _eventKey)
            {
                _animator.SetTrigger(_animatorKey);
            }
        }
    }
}