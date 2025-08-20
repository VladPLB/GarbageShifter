using UnityEngine;

namespace _GAME.Scripts.UI
{
    public abstract class UIWindow : MonoBehaviour
     {
        protected UIManager manager;

        public void SetManager(UIManager uiManager)
        {
            manager = uiManager;
        }

        public virtual void OnOpen()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnClose()
        {
            gameObject.SetActive(false);
        }

        public void Close()
        {
            if (manager != null)
                manager.CloseWindow(this);
            else
                Debug.LogWarning("UIWindow has no reference to UIManager.");
        }
    }
}