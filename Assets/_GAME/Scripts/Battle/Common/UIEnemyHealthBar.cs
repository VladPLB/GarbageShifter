using System;
using _GAME.Scripts.Pools;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Common
{
    public class UIEnemyHealthBar : MonoBehaviour, IPoolableItem<EnemySubClassType>
    {
        [SerializeField] private EnemySubClassType _type;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _image;
        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _lowColor;
        [SerializeField] private Color _criticalColor;
        
        public EnemySubClassType Type => _type;
        public RectTransform RectTransform => _rectTransform;

        public void SetValue(float val)
        {
            _slider.value = val;
            _image.color = val switch
            {
                > .8f => _normalColor,
                > .4f => _lowColor,
                _ => _criticalColor
            };
        }
        
        public void Remove()
        {
            Core.Get<PoolProvider>().UIEnemyHealthBar.Push(this);
        }
        
        public static UIEnemyHealthBar Create(EnemySubClassType type, RectTransform holder)
        {
            var item = Core.Get<PoolProvider>().UIEnemyHealthBar.Pop(type);
            item.transform.SetParent(holder);
            return item;
        }
    }
}