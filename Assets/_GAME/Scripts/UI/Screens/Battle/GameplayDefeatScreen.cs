using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class GameplayDefeatScreen : UIWindow
    {
        [SerializeField] private Button _exitButton;

        private LevelController _levelController;
        private LocalInventory _inventory;

        public override void OnOpen()
        {
            _levelController = Core.Get<LevelController>();
            _inventory = Core.Get<DropManager>().LocalInventory;
            _exitButton.SetListener(ExitGame);
            base.OnOpen();
        }

        private void ExitGame()
        {
            // all loot collected in the level is lost on defeat
            _inventory.Clear();
            Close();
            _levelController.EndLevel("level_defeat");
        }
    }
}
