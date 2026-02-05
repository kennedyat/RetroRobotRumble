using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Final Boss/OmegaGR")]
public class Omega_GR : FB_P2AttackData
{
    [Header("Burn Laser Stats")]
    public GameObject burnArea;
    public int attackCount;
    public float burnLaserLength;
    public float burnLaserWidth;
    public int burnLaserDamage;
    public float burnChannelTime;
    public float trackingLetGo;
    public int burnDamage;
    public float burnDuration;
    public float delayBetweenLasers;

    [Header("Star Laser Stats")]
    public GameObject starLaserPrefab;
    public float starLaserChannel;
    public int starLaserDamage;
    public int starLaserCount;
    public float starLaserLength;
    public float starLaserWidth;
    public float totalDegRotation;
    public float duration;
}
