using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class CommunicatorMessageEvent: IEvent
    {
        public List<string> Messages;

        public CommunicatorMessageEvent(List<string> messages) => Messages = messages;
    }
}