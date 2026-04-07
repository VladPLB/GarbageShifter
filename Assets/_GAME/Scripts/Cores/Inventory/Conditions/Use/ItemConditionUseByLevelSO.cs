using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Save;
using UnityEngine;

namespace _GAME.Scripts.Inventory.Conditions
{
    [CreateAssetMenu(menuName = "Inventory/Condition/Use/ByLevelSO", fileName = "ItemConditionUseByLevel_")]
    public class ItemConditionUseByLevelSO: ItemConditionUseSO
    {
        [SerializeField]
        private int _min = 0;
        [SerializeField]
        private int _max = 10000;

        public override bool CanUse()
        {
            var saveManager = Core.Get<SaveManager>();
            var currentLevel = saveManager.GetData<ProgressData>().Level;

            return currentLevel >= _min && currentLevel <= _max;
        }
    }
}