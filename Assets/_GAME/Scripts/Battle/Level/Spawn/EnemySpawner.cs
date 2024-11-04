using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<EnemySpawnPointsGroup> _spawnPointsGroups;

        private UnitsController _enemyController;
        private EnemyStagePreset _enemyStagePreset;
        private EnemyDatabase _enemyDatabase;
        private Coroutine _spawnCoroutine;

        private List<EnemySpawnData> _enemySpawnDatas;
        private EnemySpawnPointsGroup _group;
        private int _groupsCount;
        private List<float> _enemyGroupSpawnDelays;
        private List<float> _enemySpawnDelays;
        private List<int> _enemySpawnCounts;

        private Action<Vector3> _onSpawnGroupWarning;

        public void Setup(UnitsController enemyController, EnemyStagePreset enemyStagePreset)
        {
            _enemyController = enemyController;
            _enemyDatabase = Core.Get<DataBase>().Enemies;
            _enemyStagePreset = enemyStagePreset;
            
            StageCalculate();
        }

        private void StageCalculate()
        {
            var data = _enemyStagePreset.GetEnemiesSpawnData();
            _enemySpawnDatas = data.Item1;
            _enemyGroupSpawnDelays = data.Item2;

            _group = GetSpawnGroup(_enemySpawnDatas);
            _groupsCount = _enemySpawnDatas.Count;

            _enemySpawnCounts = _enemySpawnDatas.Select(e => e.SpawnRangeAmount.GetRandom()).ToList();
            _enemySpawnDelays = _enemySpawnDatas.Select(e => e.SpawnDelayBetwenUnits).ToList();
        }

        public void Play(Action<Vector3> spawnGroupWarningHandler)
        {
            _onSpawnGroupWarning = spawnGroupWarningHandler;
            TryClearSpawnCoroutine();
            _enemyController.PlayStage(_enemySpawnCounts.Sum());
            _spawnCoroutine = StartCoroutine(Spawn());
        }

        public void Stop()
        {
            _onSpawnGroupWarning = null;
            _enemyController.ClearCurrentEnemies();
            TryClearSpawnCoroutine();
        }

        private void TryClearSpawnCoroutine()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private IEnumerator Spawn()
        {
            for (int i = 0; i < _groupsCount; i++)
            {
                yield return new WaitForSeconds(_enemyGroupSpawnDelays[i]);
                
                EnemyType enemyType = _enemySpawnDatas[i].Type;
                EnemyClassType enemyClass = _enemyDatabase.GetClass(enemyType);
                EnemySpawnPoint spawnPoint = _group.GetSpawnPoint(enemyClass);
                _onSpawnGroupWarning?.Invoke(spawnPoint.WarningPosition);
                if (spawnPoint.TryDoorOpened())
                {
                    yield return new WaitForSeconds(.5f);
                }

                SpawnEnemies(enemyType, _enemySpawnCounts[i], _enemySpawnDelays[i], spawnPoint);
            }
        }

        private async void SpawnEnemies(EnemyType type, int count, float delay, EnemySpawnPoint spawnPoint)
        {
            for (int i = 0; i < count; i++)
            {
                var path = spawnPoint.GetPath();
                _enemyController.Spawn(type, path);
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        public EnemySpawnPointsGroup GetSpawnGroup(List<EnemySpawnData> _datas)
        {
            List<EnemyClassType> classes = new();
            foreach (var data in _datas)
            {
                EnemyClassType enemyClass = _enemyDatabase.GetClass(data.Type);
                if(!classes.Contains(enemyClass))
                    classes.Add(enemyClass);
            }

            List<EnemySpawnPointsGroup> groups = new();
            foreach (var group in _spawnPointsGroups)
            {
                if(group.ContainsSpawnerByTypes(classes))
                    groups.Add(group);
            }

            return groups.GetRandomItem();
        }
    }
}