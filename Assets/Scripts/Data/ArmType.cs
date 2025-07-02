using System;
using UnityEngine;
using System.Collections.Generic;

// Data for a type of arm. Arms do attacks.
//
// Does not specify if this is a left or right arm.
// Arms are interchangable between the two.
[CreateAssetMenu(fileName = "MyArm", menuName = "ScriptableObjects/ArmType", order = 2)]
public class ArmType : ScriptableObject
{
    public PartCommonData partCommonData;

    [SerializeField]
    public List<Abilities> abilities;

    [SerializeField]
    public List<ArmBehaviorData> armBehaviorData;
    
    //Added
    public List<StatusEffectData> statusEffectData;
    //public List<ArmBehaviorData> specialBehaviorData;
}
