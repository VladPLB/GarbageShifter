using System;
using _GAME.Scripts.Ads;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GAME.Scripts.UI.Screens.Battle
{
    // Shown on the first player death: limited time to watch a rewarded video and revive
    public class GameplayReviveScreen : UIWindow
    {
        [SerializeField] private float _duration = 20f;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private Button _declineButton;
        [SerializeField] private TextMeshProUGUI _timerLabel;
        [SerializeField] private Image _timerFill;

        private LevelController _levelController;
        private float _timeLeft;
        private bool _isWaiting;

        public override void OnOpen()
        {
            _levelController = Core.Get<LevelController>();
            _reviveButton.SetListener(OnReviveClicked);
            if (_declineButton != null)
            {
                _declineButton.SetListener(ToDefeat);
            }

            _timeLeft = _duration;
            _isWaiting = true;
            UpdateTimer();
            base.OnOpen();
        }

        private void Update()
        {
            if (!_isWaiting)
                return;

            _timeLeft -= Time.unscaledDeltaTime;
            UpdateTimer();
            if (_timeLeft <= 0f)
            {
                ToDefeat();
            }
        }

        private void UpdateTimer()
        {
            var time = Mathf.Max(0f, _timeLeft);
            if (_timerLabel != null)
            {
                _timerLabel.text = Mathf.CeilToInt(time).ToString();
            }
            if (_timerFill != null)
            {
                _timerFill.fillAmount = _duration > 0f ? time / _duration : 0f;
            }
        }

        private void OnReviveClicked()
        {
            if (!_isWaiting)
                return;

            // timer is paused while the video is shown
            _isWaiting = false;
            ShowRewardedVideo(isRewarded =>
            {
                // video not watched to the end counts as declining the revive
                if (isRewarded)
                {
                    Close();
                    _levelController.Revive();
                }
                else
                {
                    ToDefeat();
                }
            });
        }

        // callback(true) - reward granted, callback(false) - video skipped/failed
        protected virtual void ShowRewardedVideo(Action<bool> callback)
        {
            var adsManager = Core.Get<AdsManager>();
            if (adsManager == null)
            {
                callback?.Invoke(true);
                return;
            }
            adsManager.ShowRewarded("revive", callback);
        }

        private void ToDefeat()
        {
            _isWaiting = false;
            Close();
            _levelController.Defeat();
        }

        public override void OnClose()
        {
            _isWaiting = false;
            base.OnClose();
        }
    }
}
