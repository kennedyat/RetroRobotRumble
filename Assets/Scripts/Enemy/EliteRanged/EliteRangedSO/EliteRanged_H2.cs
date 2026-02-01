using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Ranged/H2")]
public class EliteRanged_H2 : EliteRangedAttackData
{
    [Header("Laser Stats")]
    public float laserLength;
    public float rotationSpeed;
    public float tickRate;
    public float recoveryTime;
}
