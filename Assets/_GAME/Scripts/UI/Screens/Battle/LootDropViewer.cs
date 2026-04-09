using _GAME.Scripts.Inventory;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class LootDropViewer: MonoBehaviour
    {
        private LocalInventory _inventory = null;
        public void Setup(LocalInventory inventory)
        {
            _inventory = inventory;
        }
    }
}