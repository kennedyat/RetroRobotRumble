using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GM2")]
public class Gungnir_M2 : FinalBossAttackData
{
    [Header("Lance Crash Down Stats")]
    public float shotDuration;
    public float beamCount;
    public float radiusAroundPlayer;
    public float crashDamage;
    public float crashChannel;
}
