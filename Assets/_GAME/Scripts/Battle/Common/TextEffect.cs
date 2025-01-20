using System;
using _GAME.Scripts.Pools;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.Common
{
    public class TextEffect : MonoBehaviour, IPoolableItem<TextEffectType>
    {
        public TextEffectType Type => _type;
        [SerializeField] private TextEffectType _type;
        [SerializeField] private TextMeshPro _label;

        [Header("AnimationParams")]
        [SerializeField]
        private Ease _ease = Ease.OutSine;
        [SerializeField]
        private float _liveTime = 1f;
        [SerializeField]
        private Vector3 _move = Vector3.up;
        [SerializeField]
        private float _upScale = 1f;

        private float _animateForce = 1f;
        private Vector2 _moveRandomize = Vector3.zero;
        public void Show(Vector3 position, string text, float force, Vector2 moveRandom)
        {
            ApplyDefaultState();
            
            transform.position = position;
            _label.text = text;
            _animateForce = force;
            _moveRandomize = moveRandom;
            gameObject.SetActive(true);
            Animation();
        }

        private void ApplyDefaultState()
        {
            transform.forward = Camera.main.transform.forward;
            transform.localScale = Vector3.one;
            var labelColor = _label.color;
            labelColor.a = 1f;
            _label.color = labelColor;
        }

        private void Animation()
        {
            var moveTo = transform.position + (_moveRandomize == Vector2.zero ? _move
                : new Vector3(_move.x + Random.Range(-_moveRandomize.x, _moveRandomize.x),
                    _move.y + Random.Range(-_moveRandomize.y, _moveRandomize.y), 0)) * _liveTime * _animateForce;

            var scaleTo = Vector3.one * _upScale * _animateForce;

            var seq = DOTween.Sequence()
                .Append(transform.DOScale(scaleTo, _liveTime).SetEase(_ease))
                .Join(transform.DOMove(moveTo, _liveTime).SetEase(_ease))
                .Join(_label.DOFade(0, _liveTime).SetEase(Ease.Linear))
                .AppendCallback(Remove);
            seq.Play();
        }

        private void Remove()
        {
            ApplyDefaultState();
            Core.Get<PoolProvider>().TextEffects.Push(this);
        }
        
        public static TextEffect Create(TextEffectType type, Vector3 position, string text, float force, Vector2 moveRandom)
        {
            var item = Core.Get<PoolProvider>().TextEffects.Pop(type);
            item.Show(position, text, force, moveRandom);
            return item;
        }
        
        public static TextEffect Create(TextEffectType type, Vector3 position, string text, float force)
        {
            return Create(type, position, text, force, Vector2.zero);
        }
        
        public static TextEffect Create(TextEffectType type, Vector3 position, string text, Vector2 moveRandom)
        {
            return Create(type, position, text, 1f, moveRandom);
        }
        
        public static TextEffect Create(TextEffectType type, Vector3 position, string text)
        {
            return Create(type, position, text, 1f, Vector2.zero);
        }
    }
}