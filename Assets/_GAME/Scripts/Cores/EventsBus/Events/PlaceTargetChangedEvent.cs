using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Events
{
    public class PlaceTargetChangedEvent: IEvent
    {
        public PlaceTarget PlaceTarget;

        public PlaceTargetChangedEvent(PlaceTarget placeTarget) => PlaceTarget = placeTarget;
    }
}
