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

}