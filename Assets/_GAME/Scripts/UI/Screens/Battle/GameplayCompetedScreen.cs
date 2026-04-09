using System;
using System.Collections;
using System.Collections.Generic;
using _GAME.Scripts;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI;
using _GAME.Scripts.UI.Screens.Battle;
using _GAME.Scripts.UI.Screens.Communications;
using UnityEngine;

public class GameplayCompetedScreen : UIWindow
{
    [SerializeField]
    private List<Animator> _animators;
    [SerializeField]
    private LootDropViewer _lootDropViewer;
    
    private LevelController _levelController;
    private DropManager _dropManager;
    
    private LocalInventory _inventory;
    
    public override void OnOpen()
    {
        InitReferences();
        InitBehaviours();
        base.OnOpen();
    }

    private void InitReferences()
    {
        _levelController = Core.Get<LevelController>();
        _dropManager = Core.Get<DropManager>();
        
        _inventory = _dropManager.LocalInventory;
    }

    private void InitBehaviours()
    {
        _lootDropViewer.Setup(_inventory);
    }
}
