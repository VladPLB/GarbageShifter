using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _GAME.Scripts
{
    public class ScenesLoader : MonoBehaviour, IRuntimeSetup
    {
        private const string uiScene = "UI";
        private const string lobbyScene = "Lobby";
        private const string gameplayScene = "Gameplay";

        private bool _isUiSceneLoaded = false;
        private string _activeMainScene;
        private bool _activeSceneLoaded = false;
        private List<string> _activeSubScenes = new();

        private void Awake()
        {
            Core.Registry(this);
        }

        public async void RuntimeSetup()
        {
            _isUiSceneLoaded = false;
            EventBus.Subscribe<KeyEvent>(OnEvent, EventBus.EventRegion.GLOBAL);
            if (!SceneManager.GetSceneByName(uiScene).isLoaded)
                await SceneManager.LoadSceneAsync(uiScene, LoadSceneMode.Additive);
            await UniTask.WaitWhile(() => !_isUiSceneLoaded);
            await LoadLobbyAsync();
        }

        public void OnEvent(KeyEvent keyEvent)
        {
            switch (keyEvent.Key)
            {
                case "SceneLoaded":
                    _activeSceneLoaded = true;
                    break;
                case "UILoaded":
                    _isUiSceneLoaded = true;
                    break;
            }
        }

        public async UniTask LoadLobbyAsync()
        {
            await SwitchMainSceneAsync(lobbyScene);
        }

        public async UniTask LoadGameplayAsync(string gameplayModeSubScene = "")
        {
            await SwitchMainSceneAsync(gameplayScene);
        }

        private async UniTask SwitchMainSceneAsync(string newMainScene, string gameplayModeSubScene = "")
        {
            EventBus.Push(new SceneLoadEvent(), EventBus.EventRegion.GLOBAL);
            _activeSceneLoaded = false;
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            if (!string.IsNullOrEmpty(_activeMainScene))
                await SceneManager.UnloadSceneAsync(_activeMainScene);
            
            foreach (var sub in _activeSubScenes)
            {
                if (SceneManager.GetSceneByName(sub).isLoaded)
                    await SceneManager.UnloadSceneAsync(sub);
            }

            _activeSubScenes.Clear();

            await SceneManager.LoadSceneAsync(newMainScene, LoadSceneMode.Additive);
            _activeMainScene = newMainScene;
            
            await LoadSubSceneAsync(gameplayModeSubScene);
            await UniTask.WaitWhile(() => !_activeSceneLoaded);
            EventBus.Push(new SceneLoadCompleteEvent(), EventBus.EventRegion.GLOBAL);
        }

        private async UniTask LoadSubSceneAsync(string subSceneName)
        {
            if(string.IsNullOrEmpty(subSceneName))
                return;
            
            if (!SceneManager.GetSceneByName(subSceneName).isLoaded)
            {
                await SceneManager.LoadSceneAsync(subSceneName, LoadSceneMode.Additive);
                _activeSubScenes.Add(subSceneName);
            }
        }
    }
}