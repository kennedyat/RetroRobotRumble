using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Melee/H1")]
public class H1_EliteMelee : EliteMeleeAttackData
{
    [Header("Lance Charge Stats")]
    public float dashTime;
    public float dashDistance;
    public float recoveryTime;
}
