using System.Collections.Generic;
using _GAME.Scripts.UI.Screens.Communications;

namespace _GAME.Scripts.Events
{
    public class OpenDialogScreenEvent: IEvent
    {
        public bool IsOpen;
        public UIDialog.PositionType PositionType;
        public OpenDialogScreenEvent(bool isOpen, UIDialog.PositionType positionType) => (IsOpen, PositionType) = (isOpen, positionType);
    }
}