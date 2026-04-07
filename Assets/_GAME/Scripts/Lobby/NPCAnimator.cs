using System;
using System.Collections.Generic;
using _GAME.Scripts.Events;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NPCAnimator : MonoBehaviour
{
    private const int IdleAltTypeCount = 9;
    
    private static readonly int IdleAltType = Animator.StringToHash("IdleAltType");
    private static readonly Vector2 IdleAltDelayRange = new Vector2(5f, 20f);
    private static Dictionary<NPCName,NPCAnimator> AnimatorsByName = new();
    private static bool _eventBusConnected = false;
    
    [SerializeField] private NPCName _name;
    [SerializeField] private Animator _animator;
    
    private float _nextAltIdlePlayTime;

    private void Start()
    {
        if (!_eventBusConnected)
        {
            EventBus.Subscribe<NpcAnimationEvent>(Play, EventBus.EventRegion.LOBBY);
            _eventBusConnected = true;
        }
        
        AnimatorsByName[_name] = this;
        UpdateIdleAnimationTiming();
    }

    private static void Play(NpcAnimationEvent e)
    {
        if (AnimatorsByName.TryGetValue(e.Name, out var animator))
        {
            animator.Play(e.Type);
        }
    }
    private void Play(NPCAnimationType type)
    {
        _animator.SetTrigger(type.ToString());
        UpdateIdleAnimationTiming();
    }

    private void Update()
    {
        if (Time.time > _nextAltIdlePlayTime)
        {
            _animator.SetFloat(IdleAltType, Random.Range(0, IdleAltTypeCount));
            Play(NPCAnimationType.IdleAlt);
        }
    }

    private void UpdateIdleAnimationTiming()
    {
        _nextAltIdlePlayTime = Time.time + Random.Range(IdleAltDelayRange.x, IdleAltDelayRange.y);
    }
}

public enum NPCName
{
    DocMeloonCry,
    DocRickMartin,
    CapBrot,
    InVaSal,
    DroidS3724
}

public enum NPCAnimationType
{
    None,
    IdleAlt,
    Yes,
    No,
    LookUp,
    LookDown,
    LookLeft,
    LookRight,
    Wave,
    Thought,
    Plead,
    Sirious,
    PhoneUse
}
