using System;
using System.Collections.Generic;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UIUnitsMarkers : MonoBehaviour
    {
        private const float OFFSET = 50f;
        
        [SerializeField] private RectTransform _holder;
        
        private UnitsController _unitsController;
        private Camera _camera;
        private Vector2 _halfScreenSize;
        private Dictionary<EnemyController, UIEnemyMarker> _markersByEnemy;

        public void Setup(UnitsController unitsController)
        {
            _unitsController = unitsController;
            _camera = Camera.main;
            _halfScreenSize = new Vector2(Screen.width / 2, Screen.height / 2);
            _markersByEnemy = new();

            _unitsController.OnSpawned += SpawnUnit;
            _unitsController.OnRemoved += RemoveUnit;
        }

        private void SpawnUnit(EnemyController enemy)
        {
            if (_markersByEnemy.ContainsKey(enemy))
                return;

            var marker = UIEnemyMarker.Create(enemy.Data.SubClass, _holder);
            _markersByEnemy.Add(enemy, marker);
        }

        private void RemoveUnit(EnemyController enemy)
        {
            if (!_markersByEnemy.ContainsKey(enemy))
                return;

            var marker = _markersByEnemy[enemy];
            marker.Remove();
            _markersByEnemy.Remove(enemy);
        }

        private void LateUpdate()
        {
            foreach (var item in _markersByEnemy)
            {
                var enemy = item.Key;
                var marker = item.Value;
                UpdateHealthBar(marker, enemy);
            }
        }

        private void UpdateHealthBar(UIEnemyMarker marker, EnemyController enemy)
        {
            Vector3 screenPoint = _camera.WorldToScreenPoint(enemy.transform.position + Vector3.up * 1.5f);
            bool isScreenContain = screenPoint.x > OFFSET && screenPoint.x < Screen.width - OFFSET &&
                                   screenPoint.y > OFFSET && screenPoint.y < Screen.height -OFFSET;
            if (screenPoint.z > 0 && !isScreenContain)
            {
                marker.gameObject.SetActive(true);
                Vector2 directionOnScreen = screenPoint - new Vector3(_halfScreenSize.x, _halfScreenSize.y, 0);
                directionOnScreen.Normalize();
                Vector3 indicatorPosition = new Vector2(
                    _halfScreenSize.x + directionOnScreen.x * (_halfScreenSize.x - OFFSET),
                    _halfScreenSize.y + directionOnScreen.y * (_halfScreenSize.y - OFFSET));

                RectTransformUtility.ScreenPointToLocalPointInRectangle(_holder, indicatorPosition, null, out var canvasPos);
                marker.transform.localPosition = canvasPos;
                
                float angle = Mathf.Atan2(directionOnScreen.y, directionOnScreen.x) * Mathf.Rad2Deg;
                marker.RectTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                marker.gameObject.SetActive(false);
            }
        }
    }
}