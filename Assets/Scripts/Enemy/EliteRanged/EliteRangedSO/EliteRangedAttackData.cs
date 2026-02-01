using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteRangedAttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public EliteRanged.AttackType attackType;

    public float attackRange;
    public int damage;

    public float channelTime;
    public float duration;
}
