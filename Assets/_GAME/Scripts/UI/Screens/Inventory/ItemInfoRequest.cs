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

    public class ItemInfoButtonOption
    {
        public ItemUseButtonView View;
        public Action Callback;
        public bool Interactable = true;
        public bool CloseOnClick;
    }

    public class ItemInfoRequest
    {
        private readonly List<ItemInfoButtonOption> _buttons = new();

        public ItemInfo Item { get; }
        public int Amount { get; set; } = 1;
        public bool ShowAmount { get; set; }
        public bool AllowPreview { get; set; } = true;
        public EquipmentStatsMode StatsMode { get; set; } = EquipmentStatsMode.None;
        public string PriceTitle { get; set; } = "Price:";
        public List<ItemAmount> Price { get; set; }
        public string PriceOverrideText { get; set; }

        public IReadOnlyList<ItemInfoButtonOption> Buttons => _buttons;

        public ItemInfoRequest(ItemInfo item)
        {
            Item = item;
        }

        public ItemInfoRequest AddButton(string label, Action callback, bool interactable = true, bool closeOnClick = false)
        {
            _buttons.Add(new ItemInfoButtonOption
            {
                View = new ItemUseButtonView(label),
                Callback = callback,
                Interactable = interactable,
                CloseOnClick = closeOnClick,
            });
            return this;
        }

        public ItemInfoRequest AddUseButtons(InventoryManager inventoryManager, Action<ItemUseResult> callback)
        {
            var item = Item;
            foreach (var action in inventoryManager.GetUseActions(item))
            {
                _buttons.Add(new ItemInfoButtonOption
                {
                    View = action.GetButtonView(item),
                    Callback = () => inventoryManager.Use(item, action, callback),
                    Interactable = action.CanExecute(item),
                });
            }
            return this;
        }
    }
}
