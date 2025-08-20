using System.Collections.Generic;
using _GAME.Scripts.UI.Screens.Communications;

namespace _GAME.Scripts.Events
{
    public class OpenTutorFadeScreenEvent: IEvent
    {
        public int Id;
        public OpenTutorFadeScreenEvent(int Id) => this.Id = Id;
    }
}