using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Ranged/L2")]
public class EliteRanged_L2 : EliteRangedAttackData
{
    [Header("Bomb Drop Stats")]
    public float explosionRadius;
    public float bombMaxHeight;
    public float bombSpinSpeed;
    public float projectileScale;
}
