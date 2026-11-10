using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.Battle.Equipment
{
    public interface IEquipmentData
    {
        string InventoryId { get; }
        InventoryCase InventoryCase { get; }
        int Level { get; }
        int MaxLevel { get; }

        void SetLevel(int level);
        string GetStatsLabel();
        string GetStatsLabelForUpgrade();
    }
}
