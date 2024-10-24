using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UIUnitsHealthBars : MonoBehaviour
    {
        [SerializeField] private RectTransform _holder;
        
        private UnitsController _unitsController;
        private Camera _camera;
        private Dictionary<EnemyController, UIEnemyHealthBar> _healthBarsByEnemy;

        public void Setup(UnitsController unitsController)
        {
            _unitsController = unitsController;
            _camera = Camera.main;
            _healthBarsByEnemy = new();

            _unitsController.OnSpawned += SpawnUnit;
            _unitsController.OnRemoved += RemoveUnit;
        }

        private void SpawnUnit(EnemyController enemy)
        {
            if (_healthBarsByEnemy.ContainsKey(enemy))
                return;

            var healthBar = UIEnemyHealthBar.Create(enemy.Data.SubClass, _holder);
            _healthBarsByEnemy.Add(enemy, healthBar);
        }

        private void RemoveUnit(EnemyController enemy)
        {
            if (!_healthBarsByEnemy.ContainsKey(enemy))
                return;

            var healthBar = _healthBarsByEnemy[enemy];
            healthBar.Remove();
            _healthBarsByEnemy.Remove(enemy);
        }

        private void LateUpdate()
        {
            foreach (var item in _healthBarsByEnemy)
            {
                var enemy = item.Key;
                var healthBar = item.Value;
                UpdateHealthBar(healthBar, enemy);
            }
        }

        private void UpdateHealthBar(UIEnemyHealthBar healthBar, EnemyController enemy)
        {
            Vector3 screenPoint = _camera.WorldToScreenPoint(enemy.HealthBarPoint);
            float healthValue = enemy.Health.Value;
            if (screenPoint.z > 0 && healthValue is > 0f and < 1f)
            {
                healthBar.SetValue(healthValue);
                healthBar.gameObject.SetActive(true);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_holder, screenPoint, null, out var canvasPos);
                healthBar.transform.localPosition = canvasPos;
            }
            else
            {
                healthBar.gameObject.SetActive(false);
            }
        }
    }
}