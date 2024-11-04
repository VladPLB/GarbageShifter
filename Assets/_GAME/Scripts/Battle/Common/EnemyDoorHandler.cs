using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoorHandler : MonoBehaviour
{
    private static readonly int OpenKey = Animator.StringToHash("Open");
    
    [SerializeField] private Animator _animator;

    private bool _isOpened = false;

    public bool Open()
    {
        if(_isOpened)
            return false;
        _animator.SetTrigger(OpenKey);
        _isOpened = true;
        return true;
    }
}
