using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Melee/L2")]
public class L2_EliteMelee : EliteMeleeAttackData
{
    [Header("Circle Slash Stats")]
    public float radius;
    public float recoveryTime;
}
