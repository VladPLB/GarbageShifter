using System;
using System.Collections;
using System.Collections.Generic;
using _GAME;
using _GAME.Scripts;
using _GAME.Scripts.Ads;
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
        ToLobby("level_complete");
    }
    
    // rewarded ad: all collected loot is taken, not only the stored part
    private void ExitWithRv()
    {
        var adsManager = Core.Get<AdsManager>();
        if (adsManager == null)
        {
            OnLootAllRewarded(true);
            return;
        }
        adsManager.ShowRewarded("loot_all", OnLootAllRewarded);
    }

    private void OnLootAllRewarded(bool isRewarded)
    {
        if (!isRewarded)
            return;

        _inventory.TakeAll();
        ToLobby(null);
    }

    private void ToLobby(string interstitialPlacement)
    {
        Close();
        _levelController.EndLevel(interstitialPlacement);
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
