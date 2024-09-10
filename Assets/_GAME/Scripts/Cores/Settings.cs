using System;
using _GAME.Scripts.Battle.Context;
using UnityEngine;

namespace _GAME.Scripts
{
    public class Settings: MonoBehaviour
    {
        [SerializeField] private BattleSettings _battleSettings;

        public BattleSettings Battle => _battleSettings;

        private void Start()
        {
            Core.Registry(this);
        }
    }
}