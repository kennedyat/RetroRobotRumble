using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EquipScreenHarness : MonoBehaviour
{
    [SerializeField]
    private BuildABotScreen _underTest;

    [SerializeField]
    private ChassisType[] _chassisType;
    [SerializeField]
    private ArmType[] _armType;
    [SerializeField]
    private LegType[] _legType;

    public void Start()
    {
        _underTest.InitFromParts(_chassisType, _armType, _legType, this);
    }
}

public partial class EquipScreenHarness : IGetSetPlayerEquips
{
    public ChassisType GetChassis() => _chassisType[0];
    public ArmType GetLeftArm() => _armType[0];
    public ArmType GetRightArm() => _armType[0];
    public LegType GetLegs() => _legType[0];

    public void SetChassis(ChassisType type) => Debug.Log("Setting Chassis " + type.ToString());
    public void SetLeftArm(ArmType type) => Debug.Log("Setting Left Arm " + type.ToString());
    public void SetRightArm(ArmType type) => Debug.Log("Setting Right Arm " + type.ToString());
    public void SetLegs(LegType type) => Debug.Log("Setting Legs " + type.ToString());
}