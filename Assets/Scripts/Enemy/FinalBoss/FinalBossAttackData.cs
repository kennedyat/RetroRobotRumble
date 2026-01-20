using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossAttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public FinalBoss.AttackTypes attackType;

    public float attackRange;
    public float damage;

    public float channelTime;
    public float duration;
}
