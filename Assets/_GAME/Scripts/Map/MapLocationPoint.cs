using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapLocationPoint : MonoBehaviour
    {
        private  Pool<MapLocationItem ,MapManager.LocationType> _pool;
        private MapLocationItem _mapLocationItem;
        private LevelLocation _data;

        public void Init( Pool<MapLocationItem ,MapManager.LocationType> pool, LevelLocation data, int locationIndex)
        {
            _pool = pool;
            _data = data;
            _mapLocationItem = _pool.Pop(_data.type);
            var itemTransform = _mapLocationItem.transform;
            itemTransform.SetParent(transform, false);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localScale = Vector3.one * .5f;
            _mapLocationItem.Init(locationIndex);
        }

        public void Clear()
        {
            _pool?.Push(_mapLocationItem);
            _mapLocationItem = null;
            _pool = null;
            _data = null;
        }
    }
}