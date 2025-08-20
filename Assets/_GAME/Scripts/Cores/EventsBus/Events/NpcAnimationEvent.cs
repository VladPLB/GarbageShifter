using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class NpcAnimationEvent: IEvent
    {
        public NPCName Name;
        public NPCAnimationType Type;

        public NpcAnimationEvent(NPCName name, NPCAnimationType type) => (Name, Type) = (name, type);

        public static void Push(NPCName name, NPCAnimationType type)
        {
            EventBus.Push(new NpcAnimationEvent(name, type), EventBus.EventRegion.LOBBY);
        }
    }
}