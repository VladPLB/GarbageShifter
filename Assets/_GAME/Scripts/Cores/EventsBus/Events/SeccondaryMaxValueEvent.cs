using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class SeccondaryMaxValueEvent: IEvent
    {
        
        public float MaxValue;

        public SeccondaryMaxValueEvent(float maxValue) => MaxValue = maxValue;
    }
}