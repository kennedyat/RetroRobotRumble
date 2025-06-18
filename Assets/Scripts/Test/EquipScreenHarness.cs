using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EquipScreenHarness : MonoBehaviour
{
    [SerializeField]
    private EquipScreen _underTest;

    [SerializeField]
    private ChassisType _chassisType;
    [SerializeField]
    private ArmType _armType;
    [SerializeField]
    private LegType _legType;

    public void Start()
    {
        _underTest.InitFromParts(new ChassisType[] { }, new[] { _armType, _armType }, new[] { _legType, _legType, _legType }, this);
    }
}

public partial class EquipScreenHarness : IGetSetPlayerEquips
{
    public ChassisType GetChassis() => _chassisType;
    public ArmType GetLeftArm() => _armType;
    public ArmType GetRightArm() => _armType;
    public LegType GetLegs() => _legType;

    public void SetChassis(ChassisType type) => Debug.Log("Setting Chassis " + type.ToString());
    public void SetLeftArm(ArmType type) => Debug.Log("Setting Left Arm " + type.ToString());
    public void SetRightArm(ArmType type) => Debug.Log("Setting Right Arm " + type.ToString());
    public void SetLegs(LegType type) => Debug.Log("Setting Legs " + type.ToString());
}