using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaTM")]
public class Omega_TM : FB_P2AttackData
{
    [Header("Triple Stab Stats")]
    public int stabCount;
    public int sweepCount;
    public float initialChannelTime;

    [Header("Stab Stats")]
    public GameObject stabHitbox;
    public float stabWidth;
    public float stabLength;
    public int stabDamage;
    public float stabWindup;
    public float stabRecovery;

    [Header("Sweep Stats")]
    public GameObject sweepHitbox;
    public float sweepRadius;
    public int sweepDamage;
    public float sweepWindup;
    public float sweepRecovery;
}
