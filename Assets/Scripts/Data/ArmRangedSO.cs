using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject for ArmTypes that use RANGED combat
/// Examples: Tiger Minigun, Shark Laser
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Part Data/Ranged")]
public class ArmRangedSO : PartComponentData
{
    public ArmRangedData rangedData;
}
