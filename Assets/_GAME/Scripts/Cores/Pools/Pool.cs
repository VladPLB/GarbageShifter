using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Pools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _GAME.Scripts
{
    public class Pool<T,TKey> where T:MonoBehaviour,IPoolableItem<TKey>
    {
        private Func<TKey, T> _getPrefabFunc;
        private Dictionary<TKey, Stack<T>> _pools = new();
        private Dictionary<TKey, Transform> _holders = new();
        private Transform _baseHolder;

        public Pool(Func<TKey,T> getPrefab)
        {
            _getPrefabFunc = getPrefab;
        }

        public T Pop(TKey key)
        {
            if (_pools.ContainsKey(key) && !_pools[key].IsNullOrEmpty())
            {
                var item = _pools[key].Pop();
                item.gameObject.SetActive(true);
                return item;
            }

            return Create(key);
        }

        private T Create(TKey key)
        {
            CheckOrCreateHolder(key);
            
            var prefab = _getPrefabFunc.Invoke(key);
            var item = Object.Instantiate(prefab, _holders[key]);
            item.gameObject.SetActive(true);
            return item;
        }

        public void Push(T item)
        {
            var key = item.Type;
            
            CheckOrCreateHolder(key);

            if (!_pools.ContainsKey(key))
            {
                _pools.Add(key, new Stack<T>());
            }
            
            item.gameObject.SetActive(false);
            item.transform.SetParent(_holders[key]);
            _pools[key].Push(item);
        }

        private void CheckOrCreateHolder(TKey key)
        {
            if (_baseHolder == null)
                _baseHolder = new GameObject($"[Pool] {typeof(T).Name}").transform;
            if (!_holders.ContainsKey(key))
            {
                var holder = new GameObject($"[{key.ToString()}]").transform;
                holder.SetParent(_baseHolder);
                _holders.Add(key, holder);
            }
        }
    }
}