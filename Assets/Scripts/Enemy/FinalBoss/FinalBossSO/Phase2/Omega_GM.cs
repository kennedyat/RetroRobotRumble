using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaGM")]
public class Omega_GM : FB_P2AttackData
{
    [Header("Lance Crash Down Stats")]
    public int beamCount;
    public float projectileScale;
    public float radiusAroundPlayer;
    public float shotTravelTime;

    [Header("Lance Charge Stats")]
    public float channelTime;
    public int chargeCount;
    public float trackingLetGo;
    public float recoveryTime;
    public float chargeSpeed;
}
