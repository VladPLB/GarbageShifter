using System;
using System.Collections.Generic;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private List<EnemyClassType> _enemies;
        [SerializeField] private EnemyDoorHandler _door;
        [SerializeField] private Transform _warningPoint;
        [SerializeField] private Bounds _spawnBox;
        [SerializeField] private List<Bounds> _targetMoveBoxes = new();
        [Header("Debug")]
        [SerializeField] private string _direction;
        [SerializeField] private Bounds _sBox;
        [SerializeField] private List<Bounds> _tBoxes = new();

        public Vector3 WarningPosition =>_warningPoint==null? _spawnBox.center : _warningPoint.position;
        public bool IsTypeContains(EnemyClassType type) => _enemies.Contains(type);

        public List<Vector3> GetPath()
        {
            List<Vector3> path = new();
            path.Add( transform.position + _spawnBox.RotateTo(transform.forward).GetRandomPoint());
            for (int i = 0; i < _targetMoveBoxes.Count; i++)
            {
                path.Add(transform.position + _targetMoveBoxes[i].RotateTo(transform.forward).GetRandomPoint());
            }

            return path;
        }

        public bool TryDoorOpened()
        {
            if (_door == null)
                return false;
            
            return _door.Open();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            _direction = transform.forward.ToString();
            _sBox = _spawnBox.RotateTo(transform.forward);
            
            Gizmos.DrawCube(transform.position + _sBox.center, _sBox.size);
            _tBoxes.Clear();
            for (int i = 0; i < _targetMoveBoxes.Count; i++)
            {
                Gizmos.color = Color.cyan;
                var tBox = _targetMoveBoxes[i].RotateTo(transform.forward);
                _tBoxes.Add(tBox);
                Gizmos.DrawCube(transform.position + tBox.center, tBox.size);
            }
        }
    }
}