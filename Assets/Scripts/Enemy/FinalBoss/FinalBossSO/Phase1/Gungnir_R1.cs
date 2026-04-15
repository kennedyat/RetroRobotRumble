using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GR1")]
public class Gungnir_R1 : FB_P1AttackData
{
    [Header("Big Laser Beam Stats")]
    public float damageTickRate;
    public float rotationSpeedBase;
    public float rotationSpeedFactor;
    public float laserRange;
    public float laserWidth;
    public float trackingLetGo;
}
