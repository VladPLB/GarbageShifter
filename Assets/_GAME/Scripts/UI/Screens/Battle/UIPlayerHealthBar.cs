using System;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Pools;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Common
{
    public class UIPlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _image;
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _lowColor;
        [SerializeField] private Color _criticalColor;

        private Player _player;

        public void Setup(Player player)
        {
            _player = player;
            SetValue(_player.Health.Value);
            _player.Health.OnChangeValue01 += SetValue;
        }

        public void SetValue(float val)
        {
            _slider.value = val;
            _image.color = val switch
            {
                > .6f => _normalColor,
                > .3f => _lowColor,
                _ => _criticalColor
            };
        }

        private void OnDisable()
        {
            if(_player!=null)
            {
                _player.Health.OnChangeValue01 += SetValue;
            }
        }
    }
}