using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using UnityEngine;

namespace _GAME.Scripts.Lobby
{
    public class AnimatorEventKeyHandler: MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private List<string> _keys;
        [SerializeField] private EventBus.EventRegion _region = EventBus.EventRegion.GAMEPLAY;
        public void OnEnable()
        {
            EventBus.Subscribe<KeyEvent>(OnEvent, _region);
        }
        
        public void OnDisable()
        {
            EventBus.Unsubscribe<KeyEvent>(OnEvent, _region);
        }
        
        public void OnEvent(KeyEvent keyEvent)
        {
            if(_keys.Contains(keyEvent.Key))
            {
                _animator.SetTrigger(keyEvent.Key);
            }
        }
    }
}