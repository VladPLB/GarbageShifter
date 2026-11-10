using System;
using System.Collections.Generic;
using _GAME.Scripts.Inventory;
using _GAME.Settings.Save;

namespace _GAME.Scripts.Cores.Save.SavesConfigs
{
    [Serializable]
    public class InventoryData : ISaveData
    {
        public bool Forced => false;

        public event Action<bool> OnDataChanged;

        public List<ItemStackSave> Items = new();
        public List<ItemUpgradeSave> Upgrades = new();
        public List<ItemCaseSave> Cases = new();
        public List<DailyClaimSave> DailyClaims = new();

        public int GameplayStorageCapacity = 3;

        public void MarkDirty(bool forced = false)
        {
            OnDataChanged?.Invoke(forced);
        }
    }

    [Serializable]
    public struct ItemStackSave
    {
        public string ItemId;
        public int Amount;
    }
    
    [Serializable]
    public struct ItemUpgradeSave
    {
        public string ItemId;
        public int Level;
    }
    
    [Serializable]
    public struct DailyClaimSave
    {
        public string Id;
        // Local calendar day number of the last claim
        public int Day;
    }

    [Serializable]
    public struct ItemCaseSave
    {
        public string ItemId;
        public InventoryCase Case;
    }

}