namespace _GAME.Scripts.Inventory
{
    public class ItemUseDescriptionInfo : ItemUseInfo
    {
        public string Text { get; }

        public ItemUseDescriptionInfo(string text)
        {
            Text = text;
        }
    }
}
