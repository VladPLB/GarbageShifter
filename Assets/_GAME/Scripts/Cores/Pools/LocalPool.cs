using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using _GAME.Scripts.Pools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _GAME.Scripts
{
    public class LocalPool<T> where T:MonoBehaviour,IPoolableItem
    {
        private T _prefab;
        private Stack<T> _pool = new();
        private Transform _holder;

        public LocalPool(T prefab) 
        {
            _prefab = prefab;
        }

        public T Pop()
        {
            if (!_pool.IsNullOrEmpty())
            {
                var item = _pool.Pop();
                return item;
            }

            return Create();
        }

        private T Create()
        {
            CheckOrCreateHolder();
            
            var item = Object.Instantiate(_prefab, _holder);
            item.gameObject.SetActive(false);
            return item;
        }

        public void Push(T item)
        {
            CheckOrCreateHolder();
            item.gameObject.SetActive(false);
            item.transform.SetParent(_holder);
            _pool.Push(item);
        }

        private void CheckOrCreateHolder()
        {
            if (_holder == null)
                _holder = new GameObject($"[Pool] {typeof(T).Name}").transform;
        }
    }
}