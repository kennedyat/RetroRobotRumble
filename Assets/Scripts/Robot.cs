using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Robot
{
    public ChassisType chassis;
    public LimbType leftArm;
    public LimbType rightArm;
    public LimbType legs;
}

