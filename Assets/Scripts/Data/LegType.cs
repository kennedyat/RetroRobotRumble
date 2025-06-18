using System;
using UnityEngine;

// Data for a type of leg(s). Legs do movement.
//
// Legs come in pairs, as of the current design.
[CreateAssetMenu(fileName = "MyLegs", menuName = "ScriptableObjects/LegsType", order = 3)]
public class LegType : ScriptableObject
{
    public PartCommonData partCommonData;
}
