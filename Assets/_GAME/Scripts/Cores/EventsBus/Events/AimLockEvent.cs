using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class AimLockEvent: IEvent
    {
        public bool IsLock;

        public AimLockEvent(bool isLock) => IsLock = isLock;
    }
}