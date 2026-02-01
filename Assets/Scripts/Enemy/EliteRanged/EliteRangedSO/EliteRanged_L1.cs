using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Ranged/L1")]
public class EliteRanged_L1 : EliteRangedAttackData
{
    [Header("Rapid Fire Stats")]
    public int projectileCount;
    public float projectileDelay;
    public float projectileLifetime;
    public float projectileSpeed;
    public float projectileScale;
    public float randomProjectileRotation;
    public float knockbackDistance;
}
