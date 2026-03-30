using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaGM")]
public class Omega_GM : FB_P2AttackData
{
    [Header("Lance Crash Down Stats")]
    public float shotDelay;
    public int shotDamage;
    public float projectileScale;
    public float projectileHeight;
    public float radiusAroundPlayer;
    public float shotTravelTime;

    [Header("Lance Charge Stats")]
    public float channelTime;
    public int chargeCount;
    public int chargeDamage;
    public float chargeSpeed;
    public float trackingLetGo;
    public float recoveryTime;
}
