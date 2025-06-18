using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Robot
{
    public ChassisType chassis;
    public ArmType leftArm;
    public ArmType rightArm;
    public LegType legs;
}

