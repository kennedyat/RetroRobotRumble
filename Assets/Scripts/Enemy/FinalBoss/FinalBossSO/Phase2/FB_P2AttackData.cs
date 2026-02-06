using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FB_P2AttackData : ScriptableObject
{
    [Header("Overall Attack Stats")]
    public GameObject projectilePrefab;

    public float attackRange;
    public float tooCloseRange;
    public int damage;
}
