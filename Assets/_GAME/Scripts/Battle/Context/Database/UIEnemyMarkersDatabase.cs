using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "UIEnemyMarkersDatabase", menuName = "Scriptable/DB/Effects/UIEnemyMarkers", order = 1)]
    public class UIEnemyMarkersDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<UIEnemyMarker> _prefabs = new();
        private Dictionary<EnemySubClassType, UIEnemyMarker> _prefabsByType = new();
        
        public void RuntimeSetup()
        {
            _prefabsByType = _prefabs.ToDictionary(b => b.Type);
        }

        public UIEnemyMarker GetPrefab(EnemySubClassType type)
        {
            return _prefabsByType[type];
        }
    }
}