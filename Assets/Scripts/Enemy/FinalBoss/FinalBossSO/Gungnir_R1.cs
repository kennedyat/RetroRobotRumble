using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GR1")]
public class Gungnir_R1 : FinalBossAttackData
{
    [Header("Big Laser Beam Stats")]
    public float speedFactor;
    public float laserRange;
    public float laserWidth;
    public float trackingLetGo;
}
