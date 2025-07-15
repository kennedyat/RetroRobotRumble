using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmBehavior
{
    // When you press left click (or right click or q or e)
    public void Activate(GameObject owner, ArmInstance arm);
    // When you release left click (or right click or q or e)
    public void Deactivate(GameObject owner, ArmInstance arm);

    // Called (by the Robot) every FixedUpdate.
    public void FixedUpdateFromArm(GameObject owner, ArmInstance arm);
}
