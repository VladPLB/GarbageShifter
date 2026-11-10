using System;

namespace _GAME.Scripts.Ads
{
    public enum AdType { Rewarded, Interstitial }

    public enum AdShowResult
    {
        Completed,  // reward earned / interstitial closed
        Skipped,    // rewarded closed before the reward
        Failed,
    }

    public interface IAdsProvider
    {
        void Initialize();
        bool IsReady(AdType type);
        void Show(AdType type, Action<AdShowResult> callback);
    }
}
