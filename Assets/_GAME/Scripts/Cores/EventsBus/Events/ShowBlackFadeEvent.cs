using UnityEngine;

namespace _GAME.Scripts.Events
{
    public class ShowBlackFadeEvent: IEvent
    {
        public bool IsShow;
        public float Duration;
        
        public ShowBlackFadeEvent(bool isShow, float duration) => (IsShow, Duration) = (isShow, duration);
    }
}