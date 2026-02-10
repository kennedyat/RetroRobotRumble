using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_P2AttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public FinalBoss.P2_Attacks attackType;

    public float attackRange;
    public int damage;

    public float channelTime;
    public float duration;
}
