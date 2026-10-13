namespace _GAME.Scripts.Inventory
{
    public class ItemUseLootInfo : ItemUseInfo
    {
        public LootTable Loot { get; }

        public ItemUseLootInfo(LootTable loot)
        {
            Loot = loot;
        }
    }
}
