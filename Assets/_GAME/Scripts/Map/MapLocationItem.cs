using _GAME.Scripts.Common;
using _GAME.Scripts.Lobby;
using _GAME.Scripts.Pools;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapLocationItem : MonoBehaviour, IPoolableItem<MapManager.LocationType>
    {
        [SerializeField] protected string _name;
        [SerializeField] protected MapManager.LocationType _type;
        [SerializeField] protected PropRandomizer _model;
        
        public MapManager.LocationType Type => _type;
        public string Name => _name;

        public void Init(int seed)
        {
            gameObject.SetActive(true);
            _model.Init(seed);
        }
    }
}