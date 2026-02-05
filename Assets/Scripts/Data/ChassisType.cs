using UnityEngine;

// Data for a type of chassis. Chassis have passives.
//
// Chassis cannot be changed mid-run.
[CreateAssetMenu(fileName = "MyChassis", menuName = "ScriptableObjects/Type/ChassisType", order = 1)]
[System.Serializable]
public class ChassisType : PartType
{ 
    [SerializeField]
    [InspectorName("ULTIMATE ABILITY")]
    public PartComponentData ultimateAbility;
    [SerializeField]
    public PartComponentData passiveAbility;
}
