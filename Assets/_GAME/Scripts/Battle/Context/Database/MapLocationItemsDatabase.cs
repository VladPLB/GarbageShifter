using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Map;
using UnityEngine;

namespace _GAME.Scripts.Battle.Context
{
    [CreateAssetMenu(fileName = "MapLocationItemsDatabase", menuName = "Scriptable/DB/Map/MapLocationItemsDatabase", order = 1)]
    public class MapLocationItemsDatabase : ScriptableObject, IRuntimeSetup
    {
        [SerializeField] private List<MapLocationItem> _prefabs = new();
        private Dictionary<MapManager.LocationType, MapLocationItem> _prefabsByType = new();
        
        public void RuntimeSetup()
        {
            _prefabsByType = _prefabs.ToDictionary(b => b.Type);
        }

        public MapLocationItem GetPrefab(MapManager.LocationType type)
        {
            return _prefabsByType[type];
        }
    }
}