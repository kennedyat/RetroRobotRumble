using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/TR1")]
public class Trishula_R1 : FinalBossAttackData
{
    [Header("Shotgun Fire Stats")]
    public GameObject projectilePrefab;
    public int projectileCount;
    public float projectileSpeed;
    public float totalDegRotation;
    public float shotDelay;
    public int attackCount;
    public float attackSequenceDelay;
    public float projLifetime;
}
