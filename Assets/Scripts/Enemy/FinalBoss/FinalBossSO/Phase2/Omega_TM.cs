using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaTM")]
public class Omega_TM : FB_P2AttackData
{
    [Serializable]
    public struct WindupAndRecovery
    {
        public float windup;
        public float recovery;
    }

    [Header("Melee Combo Stats")]
    public WindupAndRecovery[] stabTimes;
    public WindupAndRecovery[] sweepTimes;

    [Header("Stab Stats")]
    public GameObject stabHitbox;
    public float stabWidth;
    public float stabLength;
    public int stabDamage;

    [Header("Sweep Stats")]
    public GameObject sweepHitbox;
    public float sweepRadius;
    public int sweepDamage;
}
