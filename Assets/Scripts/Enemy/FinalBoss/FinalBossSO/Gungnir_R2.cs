using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GR2")]
public class Gungnir_R2 : FB_P1AttackData
{
    [Header("Burning Laser Beam Stats")]
    public float laserRange;
    public float laserWidth;
    public float trackingLetGo;
    public int burnDamage;
    public float burnDuration;
    public int attackCount;
    public float delayBetweenLasers;
}
