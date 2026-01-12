using System;
using UnityEngine;
using UnityEngine.VFX;
// Data common between Arms, Legs, and Chassis.
[Serializable]
public struct PartCommonData
{
    [Header("Basic Info")]
    public string name;
    public string description;
    
}



/// Melee-specific data
[Serializable]
public struct ArmMeleeData
{
    public float damage;
    public float knockbackForce;
    public float hitBoxActiveTime;
    public LayerMask targetLayers;
}


/// Ranged-specific data
[Serializable]
public struct ArmRangedData
{
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public float projectileRange;
    public int projectileCount;
    public float spreadAngle;
}

