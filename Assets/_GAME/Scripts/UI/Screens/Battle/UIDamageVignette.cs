using System;
using _GAME.Scripts.Battle.Player;
using DG.Tweening;
using UnityEngine;

namespace _GAME.Scripts.UI.Screens.Battle
{
    public class UIDamageVignette : MonoBehaviour
    {
        private static readonly int HitKey = Animator.StringToHash("Hit");

        [SerializeField] private Animator _animator;
        private Player _player;

        public void Setup(Player player)
        {
            _player = player;
            _player.OnDamaged += OnDamage;
        }

        private void OnDamage()
        {
            _animator.SetTrigger(HitKey);
        }
    }
}