using System;
using _GAME.Scripts.Pools;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Common
{
    public class UIMarker : MonoBehaviour, IPoolableItem<MarkerType>
    {
        [SerializeField] private MarkerType _type;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private bool _autoRemove = false;
        public MarkerType Type => _type;
        public RectTransform RectTransform => _rectTransform;

        public event Action OnRemoved;
        
        public async void AutoRemove()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            Remove();
        }

        public void Remove()
        {
            OnRemoved?.Invoke();
            OnRemoved = null;
            Core.Get<PoolProvider>().UIMarkers.Push(this);
        }
        
        public static UIMarker Create(MarkerType type, RectTransform holder)
        {
            var item = Core.Get<PoolProvider>().UIMarkers.Pop(type);
            item.transform.SetParent(holder);
            if (item._autoRemove)
            {
                item.AutoRemove();
            }
            item.gameObject.SetActive(true);
            return item;
        }
    }
}