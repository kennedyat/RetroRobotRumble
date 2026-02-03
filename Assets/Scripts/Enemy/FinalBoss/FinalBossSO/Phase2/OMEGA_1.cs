using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OMEGA_1")]
public class OMEGA_1 : FB_P2AttackData
{
    [Header("Darkness Shroud Stats")]
    public float safeSpotRadius;
    public float safetyTime;

    public int lasersDamage;
    public int laserDuration;
}
