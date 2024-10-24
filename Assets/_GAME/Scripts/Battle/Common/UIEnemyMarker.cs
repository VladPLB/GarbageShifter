using System;
using _GAME.Scripts.Pools;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Common
{
    public class UIEnemyMarker : MonoBehaviour, IPoolableItem<EnemySubClassType>
    {
        [SerializeField] private EnemySubClassType _type;
        [SerializeField] private RectTransform _rectTransform;
        public EnemySubClassType Type => _type;
        public RectTransform RectTransform => _rectTransform;

        public void Remove()
        {
            Core.Get<PoolProvider>().UIEnemyMarkers.Push(this);
        }
        
        public static UIEnemyMarker Create(EnemySubClassType type, RectTransform holder)
        {
            var item = Core.Get<PoolProvider>().UIEnemyMarkers.Pop(type);
            item.transform.SetParent(holder);
            return item;
        }
    }
}