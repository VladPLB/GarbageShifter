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
            if (keyEvent.Key == "UILoaded")
            {
                EventBus.Unsubscribe<KeyEvent>(OnEvent, EventBus.EventRegion.GLOBAL);
                _isUiSceneLoaded = true;
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
            if (!string.IsNullOrEmpty(_activeMainScene))
                await SceneManager.UnloadSceneAsync(_activeMainScene);

            await SceneManager.LoadSceneAsync(newMainScene, LoadSceneMode.Additive);
            _activeMainScene = newMainScene;

            foreach (var sub in _activeSubScenes)
            {
                if (SceneManager.GetSceneByName(sub).isLoaded)
                    await SceneManager.UnloadSceneAsync(sub);
            }

            _activeSubScenes.Clear();
            await LoadSubSceneAsync(gameplayModeSubScene);
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