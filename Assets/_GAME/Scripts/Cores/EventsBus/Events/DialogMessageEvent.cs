using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class DialogMessageEvent: IEvent
    {
        public NPCName Name;
        public List<NPCAnimationType> Animations;
        public List<string> Messages;

        public DialogMessageEvent(NPCName name, List<string> messages, List<NPCAnimationType> animations) => (Name, Messages, Animations) = (name, messages, animations);
    }
}