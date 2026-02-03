using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaGR")]
public class Omega_GR : FB_P2AttackData
{
    [Header("Star Laser Stats")]
    public int laserCount;
    public float rotationSpeed;

    [Header("Per Laser Stats")]
    public float laserLength;
    public float laserWidth;
    public float laserDamage;
}
