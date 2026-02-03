using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaTM")]
public class Omega_TM : FB_P2AttackData
{
    [Header("Triple Stab Stats")]
    public int stabCount;
    public int sweepCount;
    public float recoveryTime;

    [Header("Stab Stats")]
    public float stabWidth;
    public float stabLength;
    public int stabDamage;
    public float delayBetweenStabs;

    [Header("Sweep Stats")]
    public float stabRadius;
    public int sweepDamage;
}
