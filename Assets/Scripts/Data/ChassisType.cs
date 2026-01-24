using UnityEngine;

// Data for a type of chassis. Chassis have passives.
//
// Chassis cannot be changed mid-run.
[CreateAssetMenu(fileName = "MyChassis", menuName = "ScriptableObjects/Type/ChassisType", order = 1)]
public class ChassisType : PartType
{ 
    public PartComponentData ultimateAbility;
    public PartComponentData passiveAbility;
}
