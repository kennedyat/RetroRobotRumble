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
    private LimbType _limbType;

    public void Start()
    {
        _underTest.InitFromParts(new ChassisType[] { }, new[] { _limbType }, new[] { _limbType, _limbType }, this);
    }
}

public partial class EquipScreenHarness : IGetSetPlayerEquips
{
    public ChassisType GetChassis() => _chassisType;
    public LimbType GetLeftArm() => _limbType;
    public LimbType GetRightArm() => _limbType;
    public LimbType GetLegs() => _limbType;

    public void SetChassis(ChassisType type) => Debug.Log("Setting Chassis " + type.ToString());
    public void SetLeftArm(LimbType type) => Debug.Log("Setting Left Arm " + type.ToString());
    public void SetRightArm(LimbType type) => Debug.Log("Setting Right Arm " + type.ToString());
    public void SetLegs(LimbType type) => Debug.Log("Setting Legs " + type.ToString());
}