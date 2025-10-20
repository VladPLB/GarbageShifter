using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _GAME.Scripts.Map
{
    public class MapLocationPoint : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer _dotRenderer;
        [SerializeField]
        private Material _inactiveMaterial;
        [SerializeField]
        private Material _activeMaterial;
        [SerializeField]
        private TextMeshPro _nameLabel;
        [SerializeField]
        private TextMeshPro _progressLabel;
        [SerializeField]
        private TextMeshPro _progressLabelShadow;
        [SerializeField] 
        private string _activeColorCode = "";
        [SerializeField] 
        private string _completeColorCode = "";
        
        private  Pool<MapLocationItem ,MapManager.LocationType> _pool;
        private int _locationIndex;
        private MapLocationItem _mapLocationItem;
        private LevelLocation _data;

        public void Init( Pool<MapLocationItem ,MapManager.LocationType> pool, LevelLocation data, int locationIndex)
        {
            _pool = pool;
            _data = data;
            _locationIndex = locationIndex;
            _mapLocationItem = _pool.Pop(_data.type);
            var itemTransform = _mapLocationItem.transform;
            itemTransform.SetParent(transform, false);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localScale = Vector3.one * .5f;
            _mapLocationItem.Init(locationIndex);
        }

        public void CurrentProgress(int locationIndex, int levelIndex)
        {
            if (locationIndex != _locationIndex)
            {
                _progressLabelShadow.gameObject.SetActive(false);
                _nameLabel.gameObject.SetActive(false);
            }
            else
            {
                _nameLabel.gameObject.SetActive(true);
                _progressLabelShadow.gameObject.SetActive(true);
                _nameLabel.text = _data.type.ToString();//TODO: localize
                
                _progressLabel.text = "";
                _progressLabelShadow.text = "";
                if (levelIndex > 0)
                {
                    _progressLabel.text += $"<color={_completeColorCode}>";
                }
                for (int i = 0; i < _data.levels.Count; i++)
                {
                    _progressLabelShadow.text += "*";
                    if (i == levelIndex)
                    {
                        if (levelIndex > 0)
                        {
                            _progressLabel.text += "</color>";
                        }
                        _progressLabel.text += $"<color={_activeColorCode}>*</color>";
                    }
                    else
                    {
                        _progressLabel.text += "*";
                    }
                    
                }
            }

            _dotRenderer.transform.localScale = Vector3.one * (locationIndex == _locationIndex ?.7f:.5f);
            _dotRenderer.material = _locationIndex > locationIndex?_inactiveMaterial: _activeMaterial;
        }

        public void Clear()
        {
            _pool?.Push(_mapLocationItem);
            _mapLocationItem = null;
            _pool = null;
            _data = null;
        }
    }
}