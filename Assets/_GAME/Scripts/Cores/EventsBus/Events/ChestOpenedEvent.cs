using System.Collections.Generic;
using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Events
{
    // Pushed when a store chest is opened, before the received loot window (hook for the opening animation)
    public class ChestOpenedEvent: IEvent
    {
        public ItemInfo Chest;
        public List<ItemAmount> Loot;

        public ChestOpenedEvent(ItemInfo chest, List<ItemAmount> loot) => (Chest, Loot) = (chest, loot);
    }
}
