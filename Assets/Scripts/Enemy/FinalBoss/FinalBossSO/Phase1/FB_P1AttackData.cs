using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_P1AttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public FinalBoss.P1_Attacks attackType;
    public GameObject projectilePrefab;

    public float attackRange;
    public int damage;

    public float channelTime;
    public float duration;
}
