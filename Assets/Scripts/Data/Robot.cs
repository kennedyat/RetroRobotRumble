using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public partial struct Robot
{
    [Serializable]
    public enum Slot
    {
        CHASSIS,
        LEFT_ARM,
        RIGHT_ARM,
        LEGS,
    }

    public ChassisType chassis;
    public ArmType leftArm;
    public ArmType rightArm;
    public LegType legs;
}

// The interface is a bit silly.
// The caller could just interact directly with RunData.robot.
public partial struct Robot : IGetSetPlayerEquips
{
    public ChassisType GetChassis() => chassis;
    public ArmType GetLeftArm() => leftArm;
    public ArmType GetRightArm() => rightArm;
    public LegType GetLegs() => legs;

    public void SetChassis(ChassisType type)
    {
        chassis = type;
    }

    public void SetLeftArm(ArmType type)
    {
        leftArm = type;
    }

    public void SetRightArm(ArmType type)
    {
        rightArm = type;
    }

    public void SetLegs(LegType type)
    {
        legs = type;
    }
}