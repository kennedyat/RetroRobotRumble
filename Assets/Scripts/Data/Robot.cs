using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Robot
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

