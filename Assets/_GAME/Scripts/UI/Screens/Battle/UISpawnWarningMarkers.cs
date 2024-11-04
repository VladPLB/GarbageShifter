using System.Collections.Generic;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UISpawnWarningMarkers : MonoBehaviour
    {
        private const float OFFSET = 0f;
        
        [SerializeField] private RectTransform _holder;
        
        private LevelController _levelController;
        private Camera _camera;
        private Vector2 _halfScreenSize;
        private Dictionary<Vector3, UIMarker> _markersByPosition;

        public void Setup(LevelController levelController)
        {
            _levelController = levelController;
            _camera = Camera.main;
            _halfScreenSize = new Vector2(Screen.width / 2, Screen.height / 2);
            _markersByPosition = new();

            _levelController.OnSpawnWarning += SpawnWarningHandler;
        }

        private void SpawnWarningHandler(Vector3 position)
        {
            if (_markersByPosition.ContainsKey(position))
                return;

            var marker = UIMarker.Create(MarkerType.WARNING, _holder);
            marker.OnRemoved += () => RemoveMarker(position);
            _markersByPosition.Add(position, marker);
        }

        private void RemoveMarker(Vector3 position)
        {
            if (!_markersByPosition.ContainsKey(position))
                return;
            
            _markersByPosition.Remove(position);
        }

        private void LateUpdate()
        {
            foreach (var item in _markersByPosition)
            {
                var position = item.Key;
                var marker = item.Value;
                UpdateHealthBar(marker, position);
            }
        }

        private void UpdateHealthBar(UIMarker marker, Vector3 position)
        {
            Vector3 screenPoint = _camera.WorldToScreenPoint(position);
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