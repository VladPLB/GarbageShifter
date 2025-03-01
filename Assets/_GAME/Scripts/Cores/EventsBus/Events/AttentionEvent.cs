using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class AttentionEvent: IEvent
    {
        public string Text;

        public AttentionEvent(string text) => Text = text;
    }
}