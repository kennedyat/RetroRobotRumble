using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_P1AttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public GameObject projectilePrefab;

    public float attackRange;
    public float tooCloseRange;
    public int damage;

    public float channelTime;
    public float duration;
}
