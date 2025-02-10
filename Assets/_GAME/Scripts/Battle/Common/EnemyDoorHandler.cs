using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyDoorHandler : MonoBehaviour
{
    private static readonly int OpenKey = Animator.StringToHash("Open");
    
    [SerializeField] private Animator _animator;
    [SerializeField] private float _delay;

    private bool _isOpened = false;

    public bool Open(out float delay)
    {
        delay = 0f;
        if(_isOpened)
        {
            return false;
        }

        Open();
        delay = _delay;
        return true;
    }

    public void Open()
    {
        _animator.SetTrigger(OpenKey);
        _isOpened = true;
    }
}
