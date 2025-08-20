using System.Collections.Generic;

namespace _GAME.Scripts.Events
{
    public class SetEnableLobbyPanelEvent: IEvent
    {
        public string PanelName;
        public bool IsEnable;

        public SetEnableLobbyPanelEvent(string panelName, bool isEnable) => (PanelName, IsEnable) = (panelName, isEnable);
    }
}