using System;
using System.Collections;
using System.Collections.Generic;
using _GAME;
using _GAME.Scripts;
using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.UI;
using _GAME.Scripts.UI.Screens.Battle;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCompetedScreen : UIWindow
{
    [SerializeField]
    private LootDropViewer _lootDropViewer;
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Button _rvButton;
    
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
        
        _exitButton.SetListener(ExitGame);
        _rvButton.SetListener(ExitWithRv);
    }

    private void ExitGame()
    {
        _inventory.TakeOut();
        ToLobby();
    }
    
    private void ExitWithRv()
    {
        _inventory.TakeAll();
        ToLobby();
    }

    private void ToLobby()
    {
        Close();
        _levelController.EndLevel();
    }

    private void InitBehaviours()
    {
        _lootDropViewer.Setup(_inventory);
    }

    override public void OnClose()
    {
        _lootDropViewer.Release();
        base.OnClose();
    }
}
