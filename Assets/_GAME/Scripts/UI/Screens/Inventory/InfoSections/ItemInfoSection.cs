using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Inventory.InfoSections
{
    public abstract class ItemInfoSection : MonoBehaviour
    {
        public abstract void Setup(ItemInfoRequest request);
    }
}
