using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum EventType
{
    EnableHitBox,
    DisableHitBox,
    Soundfx,
    VFX
}
[Serializable]
public class AnimationEvent
{
    public EventType type;
    public UnityEvent animEvent;
}
