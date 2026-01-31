using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/GM2")]
public class Gungnir_M2 : FinalBossAttackData
{
    [Header("Lance Crash Down Stats")]
    public GameObject projectilePrefab;
    public float jumpHeight;

    public int beamCount;
    public float projectileScale;
    public float radiusAroundPlayer;
    public float shotTravelTime;

    public float crashChannel;
    public int crashDamage;
    public float crashScale;
    public float crashSpeed;
    public float trackingLetGo;
}
