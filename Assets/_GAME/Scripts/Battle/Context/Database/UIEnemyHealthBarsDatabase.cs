using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "UIEnemyHealthBarsDatabase", menuName = "Scriptable/DB/Effects/UIEnemyHealthBars", order = 1)]
    public class UIEnemyHealthBarsDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<UIEnemyHealthBar> _prefabs = new();
        private Dictionary<EnemySubClassType, UIEnemyHealthBar> _prefabsByType = new();
        
        public void RuntimeSetup()
        {
            _prefabsByType = _prefabs.ToDictionary(b => b.Type);
        }

        public UIEnemyHealthBar GetPrefab(EnemySubClassType type)
        {
            return _prefabsByType[type];
        }
    }
}