using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Elite Melee/H2")]
public class H2_EliteMelee : EliteMeleeAttackData
{
    [Header("Garen E Stats")]
    public float radius;
    public float spinMoveSpeed;
    public float damageTickRate;
    public float recoveryTime;
}
