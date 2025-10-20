using UnityEngine;
using UnityEngine.EventSystems;
using _GAME.Scripts.Events;

public class UIInputLocker : MonoBehaviour
{
    private readonly string _lockEventName = "LockUI";
    private readonly string _unlockEventName = "UnlockUI";

    private EventSystem _eventSystem;

    private void Awake()
    {
        EventBus.Subscribe<KeyEvent>(OnKeyEvent, EventBus.EventRegion.GLOBAL);
        CheckEventSystem();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<KeyEvent>(OnKeyEvent, EventBus.EventRegion.GLOBAL);
        Unlock();
    }

    private void OnKeyEvent(KeyEvent e)
    {
        if (e == null) return;

        if (e.Key == _lockEventName)
        {
            Lock();
        }
        else if (e.Key == _unlockEventName)
        {
            Unlock();
        }
    }

    public void Lock()
    {
        CheckEventSystem();
        if (_eventSystem == null) return;

        _eventSystem.enabled = false;
    }

    public void Unlock()
    {
        if (_eventSystem == null) return;

        _eventSystem.enabled = true;
    }

    private void CheckEventSystem()
    {
        if (_eventSystem != null) return;
        _eventSystem = EventSystem.current ?? FindObjectOfType<EventSystem>();
        if (_eventSystem == null)
        {
            Debug.LogWarning("[UIInputLocker] EventSystem не найден. Блокировка UI недоступна.");
        }
    }
}
