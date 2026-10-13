using _GAME.Scripts.Inventory;

namespace _GAME.Scripts.UI.Screens.Inventory
{
    public class ItemPreviewWindow : ItemInfoWindowBase
    {
        public static void Open(ItemInfo item)
        {
            var request = new ItemInfoRequest(item)
            {
                StatsMode = EquipmentStatsMode.Current,
                AllowPreview = false,
            };
            Core.Get<UIManager>().OpenWindow<ItemPreviewWindow>().Setup(request);
        }
    }
}
