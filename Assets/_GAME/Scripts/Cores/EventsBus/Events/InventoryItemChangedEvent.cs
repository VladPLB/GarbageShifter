using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Events
{
    public class InventoryItemChangedEvent: IEvent
    {
        public ItemInfo Item;
        public int PreviousAmount;
        public int CurrentAmount;

        public int Delta => CurrentAmount - PreviousAmount;

        public InventoryItemChangedEvent(ItemInfo item, int previousAmount, int currentAmount) =>
            (Item, PreviousAmount, CurrentAmount) = (item, previousAmount, currentAmount);
    }
}
