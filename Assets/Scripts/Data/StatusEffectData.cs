using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum TargetType
{
    Source = 1<<1 ,
    Target = 1<<2,
    AreaAroundSource = 1<<3,
    AreaAroundTarget = 1<<4
}

[CreateAssetMenu(menuName = "Effects")]
public abstract class StatusEffectData : ScriptableObject
{
    // Start is called before the first frame update
    public float damageAmount;
    public float tickSpeed;
    public float duration;
    public float knockbackAmount;
    public ParticleSystem particle;

    public TargetType targetType;

    public abstract IEffect MakeInstance();
}
