using System;
using _GAME.Scripts.Battle.Context;
using _GAME.Settings.Save;
using UnityEngine;

namespace _GAME.Scripts
{
    public class Settings: MonoBehaviour
    {
        [SerializeField] private BattleSettings _battleSettings;
        [SerializeField] private DefaultSaveDataConfig _defaultSaveData;

        public BattleSettings Battle => _battleSettings;
        public DefaultSaveDataConfig DefaultSaveData => _defaultSaveData;

        private void Start()
        {
            Core.Registry(this);
        }
    }
}