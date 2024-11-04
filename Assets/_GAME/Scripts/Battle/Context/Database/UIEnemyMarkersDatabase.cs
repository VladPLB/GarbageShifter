using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using _GAME.Scripts.Weapons.Bullets;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "UIEnemyMarkersDatabase", menuName = "Scriptable/DB/Effects/UIEnemyMarkers", order = 1)]
    public class UIMarkersDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<UIMarker> _prefabs = new();
        private Dictionary<MarkerType, UIMarker> _prefabsByType = new();
        
        public void RuntimeSetup()
        {
            _prefabsByType = _prefabs.ToDictionary(b => b.Type);
        }

        public UIMarker GetPrefab(MarkerType type)
        {
            return _prefabsByType[type];
        }
    }
}