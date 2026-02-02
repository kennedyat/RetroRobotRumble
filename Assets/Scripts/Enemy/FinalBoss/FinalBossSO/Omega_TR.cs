using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaTR")]
public class Omega_TR : FB_P2AttackData
{
    [Header("Split Projectile Sweep Stats")]
    public int projectileCount;
    public float projectileScale;
    public float shotDelay;
    public float totalDegRotation;

    public float projSpeed;
    public float projSplitDistance;

    public int splitCount;
    public float splitProjScale;
    public int splitProjDamage;
    public float splitProjLifetime;
    public float splitProjSpeed;
}
