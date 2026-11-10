using _GAME.Scripts.UI.Screens.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _GAME.Scripts.UI
{
    // Place this on a persistent GameObject in the same (non-unloaded) scene as Core/UIManager
    // so it keeps working across Lobby<->Gameplay scene switches.
    public class InventoryHotkey : MonoBehaviour, IRuntimeSetup
    {
        [SerializeField] private KeyCode _inventoryKey = KeyCode.I;
        [SerializeField] private KeyCode _shopKey = KeyCode.O;
        [SerializeField] private KeyCode _upgradeKey = KeyCode.U;
        [SerializeField] private KeyCode _storeKey = KeyCode.Y;

        private UIManager _uiManager;

        private void Awake()
        {
            Core.Registry(this, typeof(UIManager));
        }

        public void RuntimeSetup()
        {
            _uiManager = Core.Get<UIManager>();
        }

        private void Update()
        {
            if (_uiManager == null) return;
            
            var eventSystem = EventSystem.current;
            if (eventSystem != null && !eventSystem.enabled) return; // UI locked (e.g. forced dialog) - ignore the hotkey

            Toggle<InventoryWindow>(_inventoryKey);
            Toggle<StoreWindow>(_shopKey);
            Toggle<UpgradeWindow>(_upgradeKey);
            Toggle<ChestStoreWindow>(_storeKey);
        }
        
        
        private void Toggle<T>(KeyCode keyCode) where T : UIWindow
        {
            if (!Input.GetKeyDown(keyCode)) return;
            
            if (_uiManager.HasWindow<T>())
                _uiManager.CloseWindow<T>();
            else
                _uiManager.OpenWindow<T>();
        }
    }
}
