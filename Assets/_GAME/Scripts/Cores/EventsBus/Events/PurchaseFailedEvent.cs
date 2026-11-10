using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Events
{
    public class PurchaseFailedEvent: IEvent
    {
        // null if the product is unknown
        public ItemInfo Offer;
        public string Reason;

        public PurchaseFailedEvent(ItemInfo offer, string reason) => (Offer, Reason) = (offer, reason);
    }
}
