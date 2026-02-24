using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OMEGA_1")]
public class OMEGA_1 : FB_P2AttackData
{
    [Serializable]
    public struct Bounds
    {
        public float negative;
        public float positive;
    }

    [Header("Darkness Shroud Stats")]
    public Bounds xBounds;
    public Bounds zBounds;
    public float safeSpotRadius;
    public float safetyTime;

    public int laserDuration;
}
