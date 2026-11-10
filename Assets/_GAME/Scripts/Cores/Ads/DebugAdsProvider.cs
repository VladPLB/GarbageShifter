using System;
using UnityEngine;

namespace _GAME.Scripts.Ads
{
    // Editor / no SDK: every ad is "shown" instantly with the configured result
    public class DebugAdsProvider : IAdsProvider
    {
        private readonly AdShowResult _result;

        public DebugAdsProvider(AdShowResult result) => _result = result;

        public void Initialize()
        {
        }

        public bool IsReady(AdType type) => true;

        public void Show(AdType type, Action<AdShowResult> callback)
        {
            Debug.Log($"[Ads] Debug {type} ad -> {_result}");
            callback?.Invoke(_result);
        }
    }
}
