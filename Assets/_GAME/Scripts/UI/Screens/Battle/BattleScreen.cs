using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using _GAME.Scripts.UI.Screens.Battle;
using UnityEngine;

public class BattleScreen : MonoBehaviour
{
    [SerializeField] private UIAim _aim;
    [SerializeField] private UIPlayerHealthBar _playerHealthBar; 
    [SerializeField] private UIDamageVignette _damageVignette;
    [SerializeField] private UIUnitsHealthBars _unitsHealthBars;
    [SerializeField] private UIUnitsMarkers _unitsMarkers;
    [SerializeField] private UISpawnWarningMarkers _warningMarkers;
    [SerializeField] private GlitchEffectViewer _glitchEffectViewer;

    private LevelController _levelController;
    private Player _player;
    private UnitsController _unitsController;

    private void Start()
    {
        InitReferences();
        InitBehaviours();
    }
    
    private void InitReferences()
    {
        _levelController = Core.Get<LevelController>();
        _player = _levelController.Player;
        _unitsController = _levelController.UnitsController;
    }

    private void InitBehaviours()
    {
        _aim.Setup(_player);
        _playerHealthBar.Setup(_player);
        _damageVignette.Setup(_player);
        _unitsHealthBars.Setup(_unitsController);
        _unitsMarkers.Setup(_unitsController);
        _warningMarkers.Setup(_levelController);
        _glitchEffectViewer.Setup();
    }
}
