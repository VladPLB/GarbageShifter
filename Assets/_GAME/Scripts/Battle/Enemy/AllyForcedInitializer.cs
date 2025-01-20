using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Enemy
{
    public class AllyForcedInitializer: MonoBehaviour
    {
        [SerializeField] private AllyController _controller;

        public void OnInitialize()
        {
            _controller.Setup(_controller.transform.position, _controller.transform.position + _controller.transform.forward);
            _controller.Play();
        }
    }
}