using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class KeyEvent: IEvent
    {
        public string Key;

        public KeyEvent(string key) => Key = key;
    }
}