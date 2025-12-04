using System;
using System.Collections.Generic;
using UnityEngine;

// Data for a type of arm. Arms do attacks.
//
// Does not specify if this is a left or right arm.
// Arms are interchangable between the two.
[CreateAssetMenu(fileName = "MyArm", menuName = "ScriptableObjects/Type/ArmType", order = 2)]
public class ArmType : PartType
{
     public PartComponentData normalAbility;
    public PartComponentData specialAbility;

  
}
