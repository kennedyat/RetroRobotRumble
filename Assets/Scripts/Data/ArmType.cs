using System;
using System.Collections.Generic;
using UnityEngine;

// Data for a type of arm. Arms do attacks.
//
// Does not specify if this is a left or right arm.
// Arms are interchangable between the two.
[CreateAssetMenu(fileName = "MyArm", menuName = "ScriptableObjects/Type/ArmType", order = 2)]
public class ArmType : ScriptableObject
{
    public PartCommonData partCommonData;

    public GameObject BABPrefab;

    // A prefab that works completely independently.
    // To tweak values, you have to open the prefab. Yeah.
    public GameObject combatPrefab;
     public PartComponentData normalAbility;
    public PartComponentData specialAbility;

  
}
