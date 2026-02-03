using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OMEGA_2")]
public class OMEGA_2 : FB_P2AttackData
{
    [Header("Hellfire Stats")]
    public int[] pattern;
    public int numZones = 6;

    public float explosionDelay;
}
