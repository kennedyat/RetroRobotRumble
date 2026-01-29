using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Ranged/H1")]
public class EliteRanged_H1 : EliteRangedAttackData
{
    [Header("Heavy Projectile Stats")]
    public float projectileSpeed;
    public float trackingLetGo;
    public float explosionRadius;
}
