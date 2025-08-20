using UnityEngine;
using UnityEngine.EventSystems;
using _GAME.Scripts.Events;

public class UIInputLocker : MonoBehaviour
{
    private readonly string _lockEventName = "LockUI";
    private readonly string _unlockEventName = "UnlockUI";

    private EventSystem _eventSystem;

    private bool _prevEnabled = true;
    private int _lockCounter = 0;

    private void Awake()
    {
        EventBus.Subscribe<KeyEvent>(OnKeyEvent, EventBus.EventRegion.GLOBAL);
        CheckEventSystem();
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<KeyEvent>(OnKeyEvent, EventBus.EventRegion.GLOBAL);
        if (_lockCounter > 0) RestoreEventSystem();
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

        _lockCounter++;
        if (_lockCounter == 1)
        {
            _prevEnabled = _eventSystem.enabled;
            _eventSystem.enabled = false;
        }
    }

    public void Unlock()
    {
        if (_lockCounter <= 0) return;
        _lockCounter--;

        if (_lockCounter == 0)
        {
            RestoreEventSystem();
        }
    }

    private void RestoreEventSystem()
    {
        if (_eventSystem == null) return;
        _eventSystem.enabled = _prevEnabled;
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
