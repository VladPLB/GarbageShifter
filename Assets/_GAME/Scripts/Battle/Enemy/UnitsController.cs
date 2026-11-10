using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Battle.Level.Progression;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Battle.Enemy
{
    public class UnitsController : MonoBehaviour
    {
        private const float REMOVE_UNIT_TIME = 5f;
        
        private Pool<EnemyController,EnemyType> _pool;
        private DropManager _dropManager;

        private List<EnemyController> _activeUnits = new();
        private Transform _playerTransform;

        private int _enemyCountFull = 0;
        private int _enemyCount = 0;

        private EnemyBounds _enemyBounds;
        private EnemyScaling _scaling = EnemyScaling.Default;
        public int EnemyCountFull
        {
            get => _enemyCountFull;
            private set => EnemyCount = _enemyCountFull = Mathf.Max(0, value);
        }
        
        public int EnemyCount
        {
            get => _enemyCount;
            private set => _enemyCount = Mathf.Max(0, value);
        }

        public List<EnemyController> ActiveUnits => _activeUnits;

        public event Action<EnemyController> OnSpawned;
        public event Action<EnemyController> OnRemoved;

        public void Setup(Transform player, EnemyScaling scaling)
        {
            _scaling = scaling;
            _pool = Core.Get<PoolProvider>().Enemies;
            _dropManager ??= Core.Get<DropManager>();
            _activeUnits = new();
            _playerTransform = player;
        }
        
        public void PlayStage(int sum)
        {
            ClearCurrentEnemies();
            EnemyCountFull = sum;
            _enemyBounds = new EnemyBounds(_playerTransform.position + _playerTransform.forward, _playerTransform.forward);
        }
        
        public void Spawn(EnemyType type, List<Vector3> path)
        {
            var unit = _pool.Pop(type);
            unit.transform.position = path[0];
            unit.Setup(path, _playerTransform, _enemyBounds, _scaling);
            unit.OnDead += RemoveUnit;
            _activeUnits.Add(unit);
            OnSpawned?.Invoke(unit);
            unit.gameObject.SetActive(true);
            unit.Play();
        }

        public void RemoveUnit(EnemyController enemy)
        {
            RemoveUnit(enemy, false);
        }

        private async void RemoveUnit(EnemyController enemy, bool isForce)
        {
            _activeUnits.Remove(enemy);
            _dropManager.DropCoins(enemy.TargetPoint, Random.Range(3,6));
            OnRemoved?.Invoke(enemy);
            EnemyCount--;
            if(EnemyCount==0)
            {
                EventBus.Push(new KeyEvent("ClearEnemies"), EventBus.EventRegion.GAMEPLAY);
            }
            try
            {
                if (!isForce)
                {
                    var ct = this.GetCancellationTokenOnDestroy();
                    await UniTask.Delay(TimeSpan.FromSeconds(REMOVE_UNIT_TIME), cancellationToken: ct);
                }
                _pool.Push(enemy);
            }
            catch (OperationCanceledException)
            {
                // component destroyed mid-await, expected
            }
        }

        public void FreezeAll()
        {
            _activeUnits.ForEach(u => u.Freeze());
        }

        public void ClearCurrentEnemies()
        {
            _activeUnits.ForEach(u =>
            {
                u.Deactivate(true);
                OnRemoved?.Invoke(u);
                _pool.Push(u);
            });
            _activeUnits.Clear();
        }

        public void OnDestroy()
        {
            OnRemoved = null;
            OnSpawned = null;
        }

        private void OnDrawGizmos()
        {
            if (_enemyBounds != null)
            {
                _enemyBounds.DrawGizmos();
            }
        }
    }
}