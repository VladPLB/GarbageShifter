using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Ads
{
    // NoAds purchase is stored as an inventory item; it disables all ads except rewarded
    public static class NoAds
    {
        public const string ItemId = "NoAds";

        public static bool IsActive(InventoryManager inventory) => inventory != null && inventory.GetCount(ItemId) > 0;
    }
}
