using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public static class InventorySorting
    {
        // Equipped first, then higher upgrade level, then higher rank
        public static IOrderedEnumerable<T> OrderForInventory<T>(this IEnumerable<T> source, Func<T, ItemInfo> item, InventoryManager inventory)
        {
            return source
                .OrderByDescending(x => inventory.CanEquip(item(x)))
                .ThenInventoryOrder(item, inventory);
        }

        // Lower rank first, already owned equipment always at the bottom
        public static IOrderedEnumerable<T> OrderForStore<T>(this IEnumerable<T> source, Func<T, ItemInfo> item, InventoryManager inventory)
        {
            return source
                .OrderBy(x => IsOwnedEquipment(item(x), inventory))
                .ThenBy(x => item(x).Rank)
                .ThenBy(x => item(x).DisplayName);
        }

        // Affordable upgrades first, then as in the inventory
        public static IOrderedEnumerable<T> OrderForUpgrade<T>(this IEnumerable<T> source, Func<T, ItemInfo> item, InventoryManager inventory)
        {
            return source
                .OrderByDescending(x => inventory.TryCanUpgrade(item(x), out _, out _))
                .ThenByDescending(x => inventory.CanEquip(item(x)))
                .ThenInventoryOrder(item, inventory);
        }

        private static IOrderedEnumerable<T> ThenInventoryOrder<T>(this IOrderedEnumerable<T> source, Func<T, ItemInfo> item, InventoryManager inventory)
        {
            return source
                .ThenByDescending(x => inventory.GetLevel(item(x)))
                .ThenByDescending(x => item(x).Rank)
                .ThenBy(x => item(x).DisplayName);
        }

        private static bool IsOwnedEquipment(ItemInfo item, InventoryManager inventory)
        {
            return item is UpgradableItemInfo && inventory.GetCount(item) > 0;
        }
    }
}
