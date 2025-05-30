using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EquipScreenHarness : MonoBehaviour
{
    public EquipScreen yep;
    public ChassisType chassisType;
    public LimbType limbType;

    public void Start()
    {
        yep.InitFromDroppedParts(new[] { limbType }, new[] { limbType, limbType });
    }
}

public partial class EquipScreenHarness : IGetSetPlayerEquips
{
    public ChassisType GetChassis() => chassisType;
    public LimbType GetLeftArm() => limbType;
    public LimbType GetRightArm() => limbType;
    public LimbType GetLegs() => limbType;

    public void SetLeftArm(LimbType type) => Debug.Log("Setting Left Arm " + type.ToString());
    public void SetRightArm(LimbType type) => Debug.Log("Setting Right Arm " + type.ToString());
    public void SetLegs(LimbType type) => Debug.Log("Setting Legs " + type.ToString());
}