using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GR2")]
public class Gungnir_R2 : FinalBossAttackData
{
    [Header("Tracking Beam Stats")]
    public float trackingLetGo;
    public float burnDamage;
    public float burnDuration;
    public int attackCount;
}
