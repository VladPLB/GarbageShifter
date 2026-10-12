using System;
using System.Collections.Generic;
using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public enum EquipmentStatsMode
    {
        None,
        Current,
        UpgradePreview,
    }

    public class EquipmentInfoButtonOption
    {
        public string Label;
        public Action Callback;
        public bool Interactable = true;
        public bool CloseOnClick;
    }

    public class EquipmentInfoRequest
    {
        private readonly List<EquipmentInfoButtonOption> _buttons = new();

        public ItemInfo Item { get; }
        public int Amount { get; set; } = 1;
        public bool ShowAmount { get; set; }
        public EquipmentStatsMode StatsMode { get; set; } = EquipmentStatsMode.Current;
        public string PriceTitle { get; set; } = "Price:";
        public List<ItemAmount> Price { get; set; }
        public string PriceOverrideText { get; set; }

        public IReadOnlyList<EquipmentInfoButtonOption> Buttons => _buttons;

        public EquipmentInfoRequest(ItemInfo item)
        {
            Item = item;
        }

        public EquipmentInfoRequest AddButton(string label, Action callback, bool interactable = true, bool closeOnClick = false)
        {
            _buttons.Add(new EquipmentInfoButtonOption
            {
                Label = label,
                Callback = callback,
                Interactable = interactable,
                CloseOnClick = closeOnClick,
            });
            return this;
        }
    }
}
