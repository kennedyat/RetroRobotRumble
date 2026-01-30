using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Melee/L1")]
public class L1_EliteMelee : EliteMeleeAttackData
{
    [Header("Lance Thrust Stats")]
    public float width;
    public float length;
    public float recoveryTime;
}
