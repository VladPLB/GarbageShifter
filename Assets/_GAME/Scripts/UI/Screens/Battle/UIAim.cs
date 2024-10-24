using System;
using _GAME.Scripts.Battle.Player;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UIAim : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _holderCanvasGroup;
        [SerializeField] protected RectTransform _base;
        [SerializeField] protected RectTransform _dot;
        [SerializeField] protected RectTransform _hit;

        private Player _player;
        private bool _isFire = false;
        private Sequence _hitSequence = null;
        private bool _hitAnimate = false;

        public void Setup( Player player)
        {
            _player = player;
            _holderCanvasGroup.alpha = 0;
            _player.OnBattleReady += PlayerOnOnBattleReady;
            _player.OnHit += PlayerOnOnHit;
            SetBaseState();
        }

        private void PlayerOnOnHit()
        {
            if(_hitAnimate)
                return;

            _hitAnimate = true;
            _hitSequence = DOTween.Sequence().Append(
                _dot.DOLocalRotate(_dot.localRotation.eulerAngles + new Vector3(0, 0, -60), .1f))
                .Join(
                _hit.DOSizeDelta(Vector2.one * 100, .05f))
                .Join(
                _hit.DOSizeDelta(Vector2.one * 120, .05f).SetDelay(.05f)
            ).AppendCallback(
                () => _hitAnimate = false
            );
            _hitSequence.Play();
        }

        private void PlayerOnOnBattleReady(bool ready)
        {
            if (ready)
                _holderCanvasGroup.DOFade(.3f, .5f);
            else
                _holderCanvasGroup.DOFade(0f, .5f);
        }

        private void Update()
        {
            if (_player != null)
            {
                if (_isFire != _player.IsFire())
                {
                    SetBaseState();
                }
            }
        }

        private void SetBaseState()
        {
            _isFire = _player.IsFire();
            var targetSize = Vector2.one * (_isFire ? 120f : 200f);
            _base.DOSizeDelta(targetSize, .3f);
        }
    }
}