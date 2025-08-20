using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace _GAME.Scripts.UI.Screens.Communications
{
    public class SubtitlesQueueViewer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private float2 _letterPause = new float2(0.02f, 0.03f);
        [SerializeField]
        private float2 _wordPause = new float2(0.1f, 0.15f);
        [SerializeField]
        private float _messagePause = 0.2f;
        [SerializeField]
        private float _messageDuration = 2f;
        [SerializeField]
        private float _fadeOutTime = 0.5f;

        private Coroutine _typingCoroutine;
        private List<string> _messagesQueue = new();
        private bool _isFaster = false;

        public event Action OnNextMessage;
        public event Action OnEnd;

        public void Setup()
        {
            _text.text = "";
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1);
        }

        public void ShowMessage(string message)
        {
            _messagesQueue.Add(message);
            NextMessage();
        }
        
        public void ShowMessages(List<string> messages)
        {
            _messagesQueue.AddRange(messages);
            NextMessage();
        }

        private void NextMessage()
        {
            if(_messagesQueue.Count>0 && _typingCoroutine == null)
            {
                _isFaster = false;
                var message = _messagesQueue.First();
                _messagesQueue.RemoveAt(0);
                _typingCoroutine = StartCoroutine(TypeSubtitles(message));
                OnNextMessage?.Invoke();
            }
            else
            {
                _messagesQueue.Clear();
                OnEnd?.Invoke();
                OnEnd = null;
                OnNextMessage = null;
            }
        }

        public void FasterShow()
        {
            _isFaster = true;
        }

        private IEnumerator TypeSubtitles(string message)
        {
            _text.text = "";
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1);

            for (int i = 0; i < message.Length; i++)
            {
                _text.text += message[i];

                if(!_isFaster)
                {
                    if (message[i] == ' ')
                        yield return new WaitForSeconds(Random.Range(_wordPause.x, _wordPause.y));
                    else
                        yield return new WaitForSeconds(Random.Range(_letterPause.x, _letterPause.y));
                }
            }

            yield return new WaitForSeconds(_messageDuration);

            float elapsedTime = 0;
            Color originalColor = _text.color;

            while (elapsedTime < _fadeOutTime)
            {
                elapsedTime += Time.deltaTime;
                _text.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, elapsedTime / _fadeOutTime));
                yield return null;
            }

            _text.text = "";
            yield return new WaitForSeconds(_messagePause);
            _typingCoroutine = null;
            NextMessage();
        }
    }
}