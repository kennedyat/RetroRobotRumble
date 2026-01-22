using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/TR2")]
public class Trishula_R2 : FinalBossAttackData
{
    [Header("Shotgun Panic Stats")]
    public GameObject splitProjPrefab;
    public float totalDegRotation;
    public float projectileSpeed;
    public int projectileCount;
    public float shotDelay;

    public int splitCount;
    public float splitDistance;
    public float splitProjSpeed;
    public float splitProjScale;
    public int splitProjDamage;
    public float splitProjLifetime;
}
