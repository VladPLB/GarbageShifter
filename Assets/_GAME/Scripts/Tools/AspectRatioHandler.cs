using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
    [RequireComponent(typeof(CanvasScaler)), ExecuteAlways]
    public class AspectRatioHandler : MonoBehaviour
    {
        private const float BORDER_ASPECT_RATIO = 1.4f;
        [SerializeField] private CanvasScaler _canvasScaler;

        private void Awake()
        {
            UpdateCanvasScaler();
        }

        private void UpdateCanvasScaler()
        {
            _canvasScaler ??= GetComponent<CanvasScaler>();
            var aspectRatio = Screen.width / Screen.height;

            _canvasScaler.matchWidthOrHeight = aspectRatio > BORDER_ASPECT_RATIO ? 1f : 0f;
        }
#if UNITY_EDITOR
        private void Update()
        {
            UpdateCanvasScaler();
        }
#endif
    }
}
