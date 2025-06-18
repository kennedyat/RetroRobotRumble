using UnityEngine;

// Data for a type of chassis. Chassis have passives.
//
// Chassis cannot be changed mid-run.
[CreateAssetMenu(fileName = "MyChassis", menuName = "ScriptableObjects/ChassisType", order = 1)]
public class ChassisType : ScriptableObject
{
    public PartCommonData partCommonData;
}