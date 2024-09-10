using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tools
{
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform _rectTransform;
#if UNITY_EDITOR
        private Rect _lastSafeArea = Rect.zero;
#endif

        void Awake()
        {
            UpdateSafeArea();
        }

        void UpdateSafeArea()
        {
            _rectTransform ??= GetComponent<RectTransform>();
            
            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y = 0;
            anchorMax.x /= Screen.width;
            anchorMax.y = 1;
            
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
        
#if UNITY_EDITOR
        private async void Update()
        {
            Rect safeArea = Screen.safeArea;
            
            if (safeArea != _lastSafeArea)
            {
                if(Application.isPlaying)
                {
                    await UniTask.NextFrame();
                }
                UpdateSafeArea();
                _lastSafeArea = safeArea;
            }
        }
#endif
    }
}