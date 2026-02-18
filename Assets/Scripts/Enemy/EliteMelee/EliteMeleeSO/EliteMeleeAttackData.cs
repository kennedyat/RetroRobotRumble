using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteMeleeAttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public float attackRange;
    public int damage;

    public float channelTime;
    public float trackingLetGo;
    public float duration;
}