using System;
using _GAME.Scripts.Pools;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Common
{
    public class GameEffect : MonoBehaviour, IPoolableItem<GameEffectType>
    {
        public GameEffectType Type => _type;
        [SerializeField] private GameEffectType _type;
        [SerializeField] protected float _liveTime = 1f;

        public virtual async void Show(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(_liveTime));
            Remove();
        }

        protected void Remove()
        {
            Core.Get<PoolProvider>().GameEffects.Push(this);
        }
        
        public static GameEffect Create(GameEffectType type, Vector3 position)
        {
            var item = Core.Get<PoolProvider>().GameEffects.Pop(type);
            item.Show(position);
            return item;
        }
    }
}