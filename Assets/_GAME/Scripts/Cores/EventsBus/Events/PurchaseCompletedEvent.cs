using System.Collections.Generic;
using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Events
{
    public class PurchaseCompletedEvent: IEvent
    {
        public ItemInfo Offer;
        public List<ItemAmount> Loot;

        public PurchaseCompletedEvent(ItemInfo offer, List<ItemAmount> loot) => (Offer, Loot) = (offer, loot);
    }
}
