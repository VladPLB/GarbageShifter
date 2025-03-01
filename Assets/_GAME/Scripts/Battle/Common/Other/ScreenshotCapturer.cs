using System;
using System.Collections;
using _GAME.Scripts;
using UnityEngine;

public class ScreenshotCapturer : MonoBehaviour
{
    private void Awake()
    {
        Core.Registry(this);
    }

    private void OnDestroy()
    {
        Core.Unregistry(this);
    }

    public void TakeScreenshot(System.Action<Texture2D> callback)
    {
        StartCoroutine(CaptureScreenshot(callback));
    }
    
    private IEnumerator CaptureScreenshot(System.Action<Texture2D> callback)
    {
        yield return new WaitForEndOfFrame();
        
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        callback?.Invoke(screenTexture);
    }
}
